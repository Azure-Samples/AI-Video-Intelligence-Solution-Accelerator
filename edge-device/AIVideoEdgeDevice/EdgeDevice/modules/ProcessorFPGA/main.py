# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

import random
import time
import sys

import grpcserver


def main():
    print("\nPython %s\n" % sys.version)
    print("starting FPGA gRPC server...")
    server = grpcserver.ImageServicer()
    server.serve()

if __name__ == '__main__':
    main()
