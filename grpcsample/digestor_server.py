import grpc
import time
import hashlib
import digestor_pb2
import digestor_pb2_grpc
from concurrent import futures

class DigestorServicer(digestor_pb2_grpc.DigestorServicer):
    """
    gRPC server for digestor service.
    """
    def __init__(self):
        #self.server_port = 46001
        self.server_port = 28962
    
    def GetDigestor(self, request, context):
        """
        Implementation of the RPC GetDigestor declared in the proto file.
        """

        # Get the string from the incoming request
        to_be_digested = request.ToDigest

        hasher = hashlib.sha256()
        hasher.update(to_be_digested.encode())
        digested = hasher.hexdigest()

        print(digested)

        result = {'Digested': digested, 'WasDigested': True}

        return digestor_pb2.DigestedMessage(**result)

    def start_server(self):
        """
        Start the server and prepare it for servicing incoming connections.
        """

        digestor_server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        digestor_pb2_grpc.add_DigestorServicer_to_server(DigestorServicer(),
                                                         digestor_server)

        digestor_server.add_insecure_port('[::]:{}'.format(self.server_port))

        digestor_server.start()
        print("Digestor server running...")

        try:
            while True:
                time.sleep(60*60*24)
        except KeyboardInterrupt:
            print("Digestor server stopped.")


curr_server = DigestorServicer()
curr_server.start_server()
