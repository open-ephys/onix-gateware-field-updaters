#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <errno.h>
#include <fcntl.h>

#ifdef _WIN32
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

#include "riffa.h"
#define REBOOT_ADDRESS 0x0F000000
#define REBOOT_WORD 0x4F50454E

#define MODE_ADDRESS 0x0F000001
#define MODE_ONI 0x03
#define MODE_BOOTLOADER 0x02

#define IMAGE_SIZE_ADDRESS 2
#define PROGRAM_ADDRESS 3
#define STATUS_ADDRESS 4

#define BUF_SIZE 1024
#define MAX_IMAGE_SIZE 0x800000

uint32_t read_register(fpga_t* fpga, uint32_t reg)
{
    int res;
    uint32_t data = -1;
    uint32_t addr = reg | (1 << 29);
    res = fpga_send(fpga, 0, &data, 1, addr, 1, 25000);
    if (res != 1) printf("WARNING: Sent %d words",res);

    res = fpga_recv(fpga, 0, &data, 1, 25000);
    if (res != 1) printf("WARNING: Sent %d words",res);

    return data;
}

void write_register(fpga_t* fpga, uint32_t reg, uint32_t data)
{
    int res;
    uint32_t addr = reg;
    res = fpga_send(fpga, 0, &data, 1, addr, 1, 25000);
    if (res != 1) printf("WARNING: Sent %d words",res);
}

uint32_t pad_word(int fd_in, int rc0, unsigned char* data)
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

}

void show_progress(char label[], int step, int total)
{
    //progress width
    const int pwidth = 72;

    //minus label len
    int width = pwidth - strlen(label);
    int pos = (step * width) / total;

    int percent = (step * 100) / total;

    printf("%s |", label);

    // Fill progress with blocks
    for (int i = 0; i < pos; i++)  printf("%c", '\xDB');

    // Fill the rest with spaces
    printf("% *c", width - pos + 1, '|');
    printf(" %3d%%\r", percent);
}

uint32_t writewords(fpga_t* fpga, unsigned char* data, uint32_t len)
{
    uint32_t rc;
    uint32_t written = 0;
    static int count = 0;

    if (len <= 0) return 0;
    while (written < len)
    {
        rc = fpga_send(fpga, 1, data+written, len - written, 0, 1, 0);

        count++;
        if (rc < 1)
        {
            return -1;
        }
        written += rc;
    }
    return written;
}

int main (int argc, char*argv[])
{
    int fd_in;
    unsigned char buf[BUF_SIZE+4];
    uint32_t file_len, len_in_words, written = 0;
    fpga_t * fpga;
    uint32_t reg;
    int rc, rc2;
    int id;

    if (argc < 2) {
        fprintf(stderr, "Usage: %s <binary_image> [board_id]\n",argv[0]);
        exit(1);
    }

    if (argc < 3) {
        id = 0;
    } else
        id = atoi(argv[2]);

    fd_in = open(argv[1], O_RDONLY | _O_BINARY);
    if (fd_in < 0)
    {
        fprintf(stderr, "Unable to open binary image\n");
        exit(1);
    }

    file_len = lseek(fd_in, 0, SEEK_END);
    if (file_len < 0)
    {
        fprintf(stderr, "Unable to seek input file\n");
        exit(1);
    }

    if (file_len > MAX_IMAGE_SIZE)
    {
        fprintf(stderr,"Image is too big. It is %lu and the maximum supported size is %lu\n", file_len, MAX_IMAGE_SIZE);
        exit(1);
    }

    len_in_words = (file_len + 3) >> 2;
    printf("Preparing to program %lu bytes\n", file_len);
    if (lseek(fd_in, 0, SEEK_SET) < 0)
    {
        fprintf(stderr, "Unable to rewind input file\n");
        exit(1);
    }

    fpga = fpga_open(id);
    if (fpga == NULL) {
        fprintf(stderr,"Could not get FPGA %d\n", id);
        return -1;
    }

    reg = read_register(fpga, MODE_ADDRESS);
    if (reg != MODE_BOOTLOADER)
    {
        fprintf(stderr," Board not in bootloader mode.\nPlease change the mode and try again.\n");
        fpga_close(fpga);
        exit(1);
    }

    fpga_reset(fpga); //reset everything
    printf("Board %d open\n", id);

     write_register(fpga,IMAGE_SIZE_ADDRESS,file_len);
     reg = 2; // reset the flasher prior to any operation, in case it was stuck
     write_register(fpga,PROGRAM_ADDRESS,reg);
     reg = 1; //start flashing procedure
     write_register(fpga,PROGRAM_ADDRESS,reg);

    while (1)
    {
        uint32_t words;
        uint32_t res;
        rc = read(fd_in, buf, BUF_SIZE);
        if ((rc < 0) && (errno == EINTR))
            continue;

        if (rc < 0)
        {
            fprintf(stderr, "Error reading image file. Programming might not be complete\n");
            exit(1);
        }

        if (rc == 0) break;

        words = pad_word(fd_in, rc, buf + rc);
        res = writewords(fpga, buf, words);
        show_progress("Progress: ", written, len_in_words);

        if (res < 1)
        {
            fprintf(stderr,"Error writing data. Flashing operation aborted. %d\n",res);
            fpga_close(fpga);
            exit(1);
            while (1);
        }
        written += res;
        if (written > len_in_words)
        {
            fprintf(stderr,"WARNING: more bytes written than expected. %lu were written when we expected %lu.\nProgramming procedure might still finish but image might be incorrect.\n", written, (len_in_words << 2));
            break;
        }
    }
    printf("\n");

    printf("%d words (%d bytes) were written. Original file size was %d bytes\n", written, written << 2, file_len);
    printf("Waiting for programmer to finish\n");
    int return_code = 0;
    do {
        reg = read_register(fpga, STATUS_ADDRESS);
    } while ((reg & 0x08) == 0);

    if (((reg >> 4)&0x0F) != 0)
    {
        fprintf(stderr,"Programming hardware error. Code: %#6x\n",(reg >> 4)&0x0F);
        return_code = 1;
    }
    else
    {
        printf("Programming done\n");
    }

    close(fd_in);
    fpga_close(fpga);
    return return_code;
}
