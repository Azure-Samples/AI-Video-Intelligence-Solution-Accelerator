import grpc
import time
import hashlib

#import digestor_pb2
#import digestor_pb2_grpc
import fpgagrpc_pb2
import fpgagrpc_pb2_grpc

from concurrent import futures

class DigestorServicer(fpgagrpc_pb2_grpc.FpgaGrpcChannelServicer):
    """
    gRPC server for digestor service.
    """
    def __init__(self):
        #self.server_port = 46001
        self.port = 28962

    def SubmitImage(self, request, context):
        """
        Implementation of the RPC GetDigestor declared in the proto file.
        """
        print("In SubmitImage")

        # Get the string from the incoming request
        #to_be_digested = request.ToDigest

        #hasher = hashlib.sha256()
        #hasher.update(to_be_digested.encode())
        #digested = hasher.hexdigest()

        #print(digested)

        #result = {'Digested': digested, 'WasDigested': True}

        #return digestor_pb2.DigestedMessage(**result)
        rv = fpgagrpc_pb2.ImageReply()
        try:
            print("Acquiring image")
            image = request.image
            print("Image size is {} bytes".format(len(image)))
        except Exception as ex:
            print("Unexpected error:", ex)
            rv.error = "Unexpected error"
        print("Returning from SubmitImage")
        return rv

    def start_server(self):
        """
        Start the server and prepare it for servicing incoming connections.
        """

        #digestor_server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        #fpgagrpc_pb2_grpc.add_FpgaGrpcChannelServicer_to_server(DigestorServicer(),
        #                                                 digestor_server)
        fpgagrpc_pb2_grpc.add_FpgaGrpcChannelServicer_to_server(DigestorServicer(),
                                                                server)

        #digestor_server.add_insecure_port('[::]:{}'.format(self.server_port))
        print("add_insecure_port('[::]:{}')".format(self.port))
        server.add_insecure_port('[::]:{}'.format(self.port))

        #digestor_server.start()
        server.start()
        print("Digestor server running...")

        try:
            while True:
                time.sleep(60*60*24)
        except KeyboardInterrupt:
            print("Digestor server stopped.")


curr_server = DigestorServicer()
curr_server.start_server()
