#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <errno.h>
#include <fcntl.h>
#include <string.h>
#include <oni.h>
#include "sha1.h"

#ifdef _WIN32
#include <windows.h>
#include <io.h>
#define open _open
#define read _read
#define write _write
#define close _close
#define lseek _lseek
#else
#include <unistd.h>
#define _O_BINARY 0
#endif

#define BUF_WORDS 16

#define REG_HWID 0x00
#define REG_HWREV 0x01
#define REG_FWVER 0x02
#define REG_SAFEVER 0x03
#define REG_CLOCK 0x04
#define REG_REBOOT 0x10
#define REG_PROGRAM 0x20
#define REG_PROGRAM_DATA 0x21

#define PROG_DISABLE 0x00
#define PROG_ENABLE 0x01
#define PROG_BUSY 0x02
#define PROG_WAITDATA 0x04
#define PROG_EROR 0x08
#define PROG_FULL 0x10

#define BOOT_NORMAL 0x01
#define BOOT_SAFE 0x03

#define WAIT_SEC_AFTER_BOOT 5

#define MAX_FILE_SIZE 5*1024*1024 //Anything beyond this size will result in an error
const char* hdrstr = "ONIXHUBX";
const uint16_t hdrver = 1;

/*uint32_t pad_word(int fd_in, int rc0, unsigned char* data)
{
    int rem;
    int toread;
    int rc;
    uint32_t words;
        
    words = rc0 >> 2;
    rem = rc0 % 4;
    if (rem == 0) return words;
    toread = 4 - rem;
    rc = read(fd_in, data, toread);
    if (rc == toread) return (words + 1);
    else if ( rc == 0)
    {
        memset(data,0,toread);
        return words + 1;
    }
    else return pad_word(fd_in, rc0+rc, data+rc);
    
}*/

void read_data(int fd_in, char* dst, size_t size)
{
    size_t toread = size;
    size_t bytes_read = 0;
    int rc;

    do
    {
        rc = read(fd_in, dst + bytes_read, toread);
        if (rc < 0)
        {
            fprintf(stderr, "Error reading input file\n");
            exit(-1);
        }
        toread -= rc;
        bytes_read += rc;
    } while (toread > 0 && rc > 0);

    if (toread != 0) //read size mismatch
    {
        fprintf(stderr, "Read length mismatch\n");
        exit(-1);
    }
}

int main (int argc, char*argv[])
{
    int fd_in;
    uint32_t* buf;
    oni_reg_val_t val;
    int rc = ONI_ESUCCESS;
    int noprog = 0;
    int port;
    int hostid;
    size_t dev_idx;
    uint32_t written = 0;
    uint64_t file_len, len_in_words, prog_len;
    int percent = -1;
    int last_percent = -1;
    oni_ctx ctx = NULL;
    uint16_t hwid, hwid_dev, hwrev, hwrev_dev, fwver, fwver_dev;
    
    if (argc < 3)
    {
        printf("Usage: %s <host_id> <hub_port> [binary_image]\n",argv[0]);
        exit(-1);
    }

    hostid = atoi(argv[1]);

    port = atoi(argv[2]);
    dev_idx = ((int)((port + 1) & 0x000000FF) << 8) + 254;
    if (argc < 4)
    {
        printf("No programming file specified. Information-only mode\n");
        noprog = 1;
    }
    
    // Generate context
    ctx = oni_create_ctx("riffa");
    if (!ctx) {
        printf("Error creating context\n");
        exit(-1);
    }
    
    // Initialize context and discover hardware
    rc = oni_init_ctx(ctx, hostid);
    if (rc) { 
        printf("Error initializing context: %s\n", oni_error_str(rc)); 
        oni_destroy_ctx(ctx);
        exit(-1);
    }

    
    rc = oni_read_reg(ctx, dev_idx,REG_HWID,&val);
    if (rc != ONI_ESUCCESS)
    {
        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
        fprintf(stderr, "Hub might not be connected. Trying to read device %d\n", dev_idx);
        oni_destroy_ctx(ctx);
        exit(-1);
    }
    hwid_dev = val & 0xFFFF;
    printf("Safe image: %d\n",(val & 0x00010000) > 16);
    printf("Device id: 0x%04X\n",hwid_dev);
    
    rc = oni_read_reg(ctx, dev_idx,REG_HWREV,&val);
    if (rc != ONI_ESUCCESS)
    {
        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
        oni_destroy_ctx(ctx);
        exit(-1);
    }
    hwrev_dev = val & 0xFFFF;
    printf("Hardware revision: 0x%04X\n",val&0xFFFF);
    rc = oni_read_reg(ctx, dev_idx,REG_FWVER,&val);
    if (rc != ONI_ESUCCESS)
    {
        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
        oni_destroy_ctx(ctx);
        exit(-1);
    }
    fwver_dev = val & 0xFFFF;
    printf("Firmware version: 0x%04X\n",val&0xFFFF);
    rc = oni_read_reg(ctx, dev_idx,REG_SAFEVER,&val);
    if (rc != ONI_ESUCCESS)
    {
        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
        oni_destroy_ctx(ctx);
        exit(-1);
    }
    printf("Safe firmware version: 0x%04X\n",val&0xFFFF);
    rc = oni_read_reg(ctx, dev_idx,REG_CLOCK,&val);
    if (rc != ONI_ESUCCESS)
    {
        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
        oni_destroy_ctx(ctx);
        exit(-1);
    }
    printf("Clock frequency: %uHz\n",val);

    //Programming mode enable
    if (!noprog)
    {
        fd_in = open(argv[3], O_RDONLY | O_BINARY);
        if (fd_in < 0)
        {
            fprintf(stderr, "Unable to open binary image\n");
            oni_destroy_ctx(ctx);
            exit(-1);
        }
        file_len = lseek(fd_in, 0, SEEK_END);
        if (file_len < 0)
        {
            fprintf(stderr, "Unable to seek input file\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }
        if (lseek(fd_in, 0, SEEK_SET) < 0)
        {
            fprintf(stderr, "Unable to rewind input file\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        char onixstring[9];
        read_data(fd_in, onixstring, 8);
        onixstring[8] = '\0';
        if (strncmp(onixstring, hdrstr, 8) != 0)
        {
            fprintf(stderr, "Invalid file header\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        uint16_t ver;
        read_data(fd_in, &ver, 2);
        if (ver != hdrver)
        {
            fprintf(stderr, "Invalid file version\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        read_data(fd_in, &hwid, 2);
        if (hwid != hwid_dev)
        {
            fprintf(stderr, "Invalid device type\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        read_data(fd_in, &hwrev, 2);
        if (hwrev < hwrev_dev)
        {
            fprintf(stderr, "Invalid device revision\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        read_data(fd_in, &fwver, 2);
        read_data(fd_in, &prog_len, 8);
        if (prog_len != (file_len - 64))
        {
            fprintf(stderr, "File size mismatch. Possible corruption\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        unsigned char hash[20], hashr[20];
        SHA1_CTX shactx;

        read_data(fd_in, hashr, 20);

        if (prog_len > MAX_FILE_SIZE)
        {
            fprintf(stderr, "Bitfile exceeds supported size\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }
        
        len_in_words = (prog_len + 3) >> 2;

        if (lseek(fd_in, 64, SEEK_SET) < 0)
        {
            fprintf(stderr, "Unable to seek input file to bitfile data\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }

        //we allocate a 32-bit aligned chunk, which is what we will write
        buf = malloc(len_in_words * sizeof(uint32_t));
        if (buf == NULL)
        {
            fprintf(stderr, "Unable to allocate memory for bitfile\n");
            oni_destroy_ctx(ctx);
            exit(1);
        }
        memset(buf, 0xFF, len_in_words * sizeof(uint32_t));
        read_data(fd_in, (char*)buf, prog_len);
        close(fd_in);

        //calculate checksum
        SHA1Init(&shactx);
        SHA1Update(&shactx, (unsigned char const*)buf, prog_len);
        SHA1Final(hash, &shactx);

        for (int i = 0; i < 20; i++)
        {
            if (hash[i] != hashr[i])
            {
                fprintf(stderr, "Invalid hash. Possible corruption\n");
                oni_destroy_ctx(ctx);
                free(buf);
                exit(1);
            }
        }

        printf("\n\nPreparing to program bitfile.\nCurrent FW version: %#06X\nFile reported FW version: %#06X\nFile bitfile hash: ", fwver_dev, fwver);
        for (int i = 0; i < 20; i++)
            printf("%02X", hash[i]);

        printf("\n\nAre you sure you want to continue ? [y / n]");
        fflush(stdin);
        char r = getc(stdin);
        if (!(r == 'y' || r == 'Y'))
        {
            printf("\nAborting\n");
            oni_destroy_ctx(ctx);
            free(buf);
            exit(1);
        }

        printf("\nStarting programming. Do not disconnect the hub or power down the computer.\n");
        
        // Be sure that the programmer is in disabled state
        rc = oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
        if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error writing register\n",__LINE__);
            oni_destroy_ctx(ctx);
            free(buf);
            exit(-1);
        }
        rc = oni_read_reg(ctx, dev_idx,REG_PROGRAM,&val);
        if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error reading register\n",__LINE__);
            oni_destroy_ctx(ctx);
            free(buf);
            exit(-1);
        }
        if ( (val & PROG_BUSY) != 0)
        {
            printf("Programmer busy. Try again in a few seconds or reboot the device.\n");
            oni_destroy_ctx(ctx);
            free(buf);
            exit(-1);
        }
        
        rc = oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_ENABLE);
        if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error writing register\n",__LINE__);
            oni_destroy_ctx(ctx);
            free(buf);
            exit(-1);
        }
        do {
            rc = oni_read_reg(ctx, dev_idx,REG_PROGRAM,&val);
            if (rc != ONI_ESUCCESS)
            {
                fprintf(stderr,"(%d) Error reading register\n",__LINE__);
                oni_destroy_ctx(ctx);
                free(buf);
                exit(-1);
            }
        } while ( (val & PROG_WAITDATA) == 0);
        if ( (val & PROG_EROR ) != 0)
        {
            printf("Error while clearing flash. Aborting\n");
            oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
            oni_destroy_ctx(ctx);
            free(buf);
            exit(-1);
        }
        
        while (written < len_in_words)
        {  
            uint32_t words;
            
            words = ((len_in_words - written) < BUF_WORDS) ? (len_in_words - written) : BUF_WORDS;
            for (uint32_t i = 0; i < words; i++)
            {
                
                rc = oni_write_reg(ctx, dev_idx,REG_PROGRAM_DATA,buf[written+i]);
                if (rc != ONI_ESUCCESS)
                {
                    fprintf(stderr,"Error writing flash data. Flash operation aborted\n");
                    oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
                    oni_destroy_ctx(ctx);
                    free(buf);
                    exit(-1);
                }            
            }
            do {
                    rc = oni_read_reg(ctx, dev_idx,REG_PROGRAM,&val);
                    if (rc != ONI_ESUCCESS)
                    {
                        fprintf(stderr,"(%d) Error reading register\n",__LINE__);
                        oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
                        oni_destroy_ctx(ctx);
                        free(buf);
                        exit(-1);
                    }
                } while ( ( val & PROG_WAITDATA) == 0); //Wait until FIFO is clear to send another packet
                if ( (val & PROG_EROR ) != 0)
                {
                    printf("Error while writing flash. Aborting\n");
                    oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
                    oni_destroy_ctx(ctx);
                    free(buf);
                    exit(-1);
                }
            written += words;
            percent = 100*written/len_in_words;
            if (percent > last_percent)
                printf("Writing (%d%%)\n", percent);
            last_percent = percent;
        }
        printf("Sent %zd bytes to programmer\n",written*sizeof(uint32_t));
        free(buf);
        rc = oni_write_reg(ctx, dev_idx,REG_PROGRAM,PROG_DISABLE);
        if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error writing register\n",__LINE__);
            oni_destroy_ctx(ctx);
            exit(-1);
        }
        printf("Waiting for programmer to finish\n");
        do
        {
            rc = oni_read_reg(ctx, dev_idx,REG_PROGRAM,&val);
            if (rc != ONI_ESUCCESS)
            {
                fprintf(stderr,"(%d) Error reading register\n",__LINE__);
                oni_destroy_ctx(ctx);
                exit(-1);
            }
        } while ( (val & PROG_BUSY) != 0);
        printf("Programming successful!\nRebooting headstage. Please wait.\n");
        rc = oni_write_reg(ctx, dev_idx,REG_REBOOT,BOOT_NORMAL);
    /*    if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error writing register\n",__LINE__);
            oni_destroy_ctx(ctx);
            close(fd_in);
            exit(-1);
        }*/
#ifdef _WIN32
        Sleep(5000);
#else
        sleep(5);
#endif
        rc = oni_read_reg(ctx, dev_idx,REG_HWID,&val);
        if (rc != ONI_ESUCCESS)
        {
            fprintf(stderr,"(%d) Error reading register\n",__LINE__);
            oni_destroy_ctx(ctx);
            exit(-1);
        }
        if ((val & 0x00010000) == 0)
        {
            printf("Reboot into new image successful!\n");

            rc = oni_read_reg(ctx, dev_idx,REG_FWVER,&val);
            if (rc != ONI_ESUCCESS)
            {
                fprintf(stderr,"(%d) Error reading register\n",__LINE__);
                oni_destroy_ctx(ctx);
                exit(-1);
            }
            printf("New firmware version: 0x%04X\n",val&0xFFFF);
        }
        else printf("Error booting into new firmware. Safe image loaded. Check the firmware file and try again.\n");
         
    }
    oni_destroy_ctx(ctx);
    
    return 0;
};
    