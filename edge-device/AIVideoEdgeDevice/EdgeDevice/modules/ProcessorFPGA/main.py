# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

import random
import time
import sys

import grpcserver



def main():
    try:
        print ( "\nPython %s\n" % sys.version )

        print("starting FPGA gRPC server...")
        server = grpcserver.ImageServicer()
        server.start_server()
        print("server started.")

        while True:
            time.sleep(60*60*24)

    except KeyboardInterrupt:
        print ( "IoTHubModuleClient sample stopped" )

if __name__ == '__main__':
    main()
