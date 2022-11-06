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

int main(int argc, char** argv) {

    fpga_t * fpga;
    enum { BOOTLOADER, ONI } mode = BOOTLOADER;
    uint32_t data;
    int id;

    if (argc < 2) {
        id = 0;
    }
    else {
        id = atoi(argv[1]);
    }

    fpga = fpga_open(id);

    if (fpga == NULL) {
        printf("Could not get FPGA %d\n", id);
        return -1;
    }

    data = read_register(fpga, MODE_ADDRESS);

    if (data == MODE_BOOTLOADER) {
        mode = BOOTLOADER;
    }
    else if (data == MODE_ONI) {
        mode = ONI;
    }
    else {
        printf("Incorrect mode value: %d\n", data);
        fpga_close(fpga);
        return -1;
    }

    if (mode == BOOTLOADER)
    {
        printf("Board is in bootloader mode. Preparing to reboot into normal mode\n");
        printf("Check that all programming operations have finished\n");
        printf("If, after rebooting, the board persists in bootloader mode then the ONI image might be damaged and will need reflashing over JTAG\n");
    }
    else {
        printf("Board is in normal operation mode. Preparing to reboot into bootloader mode\n");
        printf("Check that all operarions are finished and the device is not in use\n\n");
    }

    char yn[100];
    printf("Proceed [y/n]? ");
    fgets(&yn, 100, stdin);

    if (!strcmp(yn, "y\n\0")) {
        write_register(fpga, REBOOT_ADDRESS, REBOOT_WORD);
        fpga_reset(fpga);
        printf("Mode change complete.\n");
    } else {
        printf("Mode not changed.\n");
    }

    fpga_close(fpga);

    return 0;
}