/********************************************************
* ONIX hub bitfile preparation software
* Written by: Aarón Cuevas López
* 
* This software prepares a MAX10 ONI hub bitfile by
* - Trimming all excess empty space at the end of the file
* - Creating a header
* 
* HEADER FORMAT v1 Total size: 64 bytes
* char[8]: string "ONIXHUBX"
* uint16: file version (current version 01)
* uint16: target hardware ID
* uint16: target min hardware revision
* uint16: current firmware version
* uint64: bitfile length
* char[20]: SHA-1 hash of bitfile
* char[20]: 0 padding for future use
* 
* File usage:
* bf_prep <hardware_id> <min_hardware_rev> <firmware_version> <input_bitfile> <output_file>
* 
* hardware_id, min_hardware_rev and firmware_version in hex format 
* Example:
* bf_prep 0002 0103 0002 bifile.rpd
********************************************************/

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <errno.h>
#include <fcntl.h>
#include "sha1.h"

#ifdef _WIN32
#include <windows.h>
#include <io.h>
#include <sys/stat.h>
#define open _open
#define read _read
#define write _write
#define close _close
#define lseek _lseek
#define DPERMS _S_IREAD | _S_IWRITE
#else
#include <unistd.h>
#define _O_BINARY 0
#define DPERMS S_IRUSR  | S_IWUSR  | S_IRGRP  | S_IROTH  
#endif

#define MAX_FILE_SIZE 5*1024*1024 //Anything beyond this size will result in an error
const uint16_t headerver = 1;
const char* headerstr = "ONIXHUBX";

int main(int argc, char* argv[])
{
	uint16_t hwid, minhwrev, fwver;
	int fd_in, fd_out;
	size_t file_len, trimmed_len;
	char* filemem;

	SHA1_CTX ctx;
	unsigned char hash[20];

	if (argc < 6)
	{
		printf("Usage: %s <hardware_id> <min_hardware_rev> <firmware_version> <input_bitfile> <output_file>\n", argv[0]);
		exit(-1);
	}

	hwid = (uint16_t)strtoul(argv[1], NULL, 16);
	minhwrev = (uint16_t)strtoul(argv[2], NULL, 16);
	fwver = (uint16_t)strtoul(argv[3], NULL, 16);

	fd_in = open(argv[4], O_RDONLY | O_BINARY);
	if (fd_in < 0)
	{
		fprintf(stderr, "Unable to open source bitfile\n");
		exit(-1);
	}
	
	file_len = lseek(fd_in, 0, SEEK_END);
	if (file_len < 0)
	{
		fprintf(stderr, "Unable to seek input file\n");
		exit(-1);
	}
	if (lseek(fd_in, 0, SEEK_SET) < 0)
	{
		fprintf(stderr, "Unable to rewind input file\n");
		exit(-1);
	}

	//A memory mapped file would be the best option here, but windows likes to make that difficult
	//In any case these files are small, so we can just load them into memort

	if (file_len > MAX_FILE_SIZE)
	{
		fprintf(stderr, "Input file size too large. %llu bytes. Maximum supported is %llu bytes.\n", (unsigned long long)file_len, (unsigned long long)MAX_FILE_SIZE);
		exit(-1);
	}
	filemem = malloc(file_len);
	if (filemem == NULL)
	{
		fprintf(stderr, "Unable to allocate file memory.\n");
		exit(-1);
	}

	size_t toread = file_len;
	size_t bytes_read = 0;
	int rc;

	do
	{
		rc = read(fd_in, filemem + bytes_read, toread);
		if (rc < 0)
		{
			fprintf(stderr, "Error reading input file\n");
			free(filemem);
			exit(-1);
		}
		toread -= rc;
		bytes_read += rc;
	} while (toread > 0 && rc > 0);

	if (toread != 0) //read size mismatch
	{
		fprintf(stderr, "Read length mismatch\n");
		free(filemem);
		exit(-1);
	}
	close(fd_in);
	
	
	unsigned char a;
	trimmed_len = file_len;
	do
	{
		trimmed_len -= 1;
		a = *(filemem + trimmed_len);
	} while ((unsigned char)*(filemem + trimmed_len) == 0xFF);

	trimmed_len = 4 *(size_t)(trimmed_len / 4 +3); //align at 32bit word boundaries and add two extra 32bit word as safety
	if (trimmed_len > file_len) trimmed_len = file_len;

	//calculate checksum
	SHA1Init(&ctx);
	SHA1Update(&ctx, (unsigned char const*)filemem, trimmed_len);
	SHA1Final(hash, &ctx);

	printf("File info:\nHardware id: %#06X\nMinimum hardware revision: %#06X\nFirmware version: %#06X\nOriginal length: %llu\nTrimmed length: %llu\nSHA-1 hash: ",
		hwid, minhwrev, fwver, (unsigned long long)file_len, (unsigned long long)trimmed_len);
	
	for (int i = 0; i < 20; i++)
		printf("%02X", hash[i]);

	printf("\n\n");

	//Write the header
	fd_out = open(argv[5], O_BINARY | O_WRONLY | O_CREAT | O_TRUNC, DPERMS);
	if (fd_out < 0)
	{
		fprintf(stderr, "Error opening destination file error %i\n", errno);
		free(filemem);
		exit(-1);
	}

	//create a header in a single array and write it at once
	char header[64];
	uint64_t size = trimmed_len;
	memset(header, 0, 64);
	memcpy(header, headerstr, 8);
	memcpy(header + 8, &headerver, 2);
	memcpy(header + 10, &hwid, 2);
	memcpy(header + 12, &minhwrev, 2);
	memcpy(header + 14, &fwver, 2);
	memcpy(header + 16, &size, 8);
	memcpy(header + 24, hash, 20);

	size_t written = 0;
	size_t towrite = 64;
	do
	{
		rc = write(fd_out, (header + written), towrite);
		if (rc < 0)
		{
			fprintf(stderr, "Error writing to destination file\n");
			free(filemem);
			exit(-1);
		}
		towrite -= rc;
		written += rc;
	} while (towrite > 0);

	written = 0;
	towrite = trimmed_len;
	do
	{
		rc = write(fd_out, (filemem + written), towrite);
		if (rc < 0)
		{
			fprintf(stderr, "Error writing to destination file\n");
			free(filemem);
			exit(-1);
		}
		towrite -= rc;
		written += rc;
	} while (towrite > 0);
	printf("Update file creation successful\n");
	free(filemem);
	close(fd_out);
	
	return 0;
}