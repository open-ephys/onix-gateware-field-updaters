#include <stdlib.h>
#include <stdio.h>
#include <windows.h>
#include <stdint.h>
#include "riffa.h"

#define REBOOT_ADDRESS 0x0F000000
#define REBOOT_WORD 0x4F50454E

#define MODE_ADDRESS 0x0F000001
#define MODE_ONI 0x03
#define MODE_BOOTLOADER 0x02

#define BOOTLOADER_HWVERSION_ADDRESS 0x0
#define BOOTLOADER_FWVERSION_ADDRESS 0x1

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

int main(int argc, char** argv) {
    fpga_t * fpga;
    enum { BOOTLOADER, ONI } mode = BOOTLOADER;
    uint32_t data;

    int id;

    if (argc < 2) {
        id = 0;
    } else
        id = atoi(argv[1]);

    fpga = fpga_open(id);
    if (fpga == NULL) {
        printf("Could not get FPGA %d\n", id);
        return -1;
    }
    data = read_register(fpga, MODE_ADDRESS);
    if (data == MODE_BOOTLOADER) mode = BOOTLOADER;
    else if (data == MODE_ONI) mode = ONI;
    else
    {
        printf("Incorrect mode value: %d\n",data);
        fpga_close(fpga);
        return -1;
    }

    if (mode == BOOTLOADER)
    {
        printf("Board in bootloader mode. \n");
        data = read_register(fpga, BOOTLOADER_HWVERSION_ADDRESS);
        uint32_t hwid = (data >> 16) & 0xFFFF;
        uint32_t hwrev = data & 0xFFFF;
        data = read_register(fpga, BOOTLOADER_FWVERSION_ADDRESS);
        printf("Hardware ID: %d\n\Hardware rev: %d\nBootloader version: %d\n", hwid, hwrev, data);

    } else {
        printf("Board in normal operation mode. \n");
    }

    printf("Press any key to exit...\n");
    getchar();

    fpga_close(fpga);
    return 0;
}