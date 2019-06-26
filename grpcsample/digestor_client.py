import grpc
#import digestor_pb2
#import digestor_pb2_grpc
import fpgagrpc_pb2
import fpgagrpc_pb2_grpc

class DigestorClient(object):
    """
    Client for accessing gRPC server.
    """
    def __init__(self):
        # Configure the host and the port to which the
        # server will connect.
        self.host = 'localhost'
        #self.port = 46001
        self.port = 28962

        # Create a communication channel.
        self.channel = grpc.insecure_channel("{}:{}".format(self.host, self.port))

        # Bind the client to the server channel
        self.stub = fpgagrpc_pb2_grpc.FpgaGrpcChannelStub(self.channel)

    def get_digest(self, message):
        """
        Client function to call the RPC for GetDigest
        """
        #to_digest_message = digestor_pb2.DigestMessage(ToDigest=message)
        #return self.stub.GetDigestor(to_digest_message)
        image = bytes()
        to_digest_message = fpgagrpc_pb2.ImageBody(image=image)
        return self.stub.SubmitImage(to_digest_message)

if __name__ == "__main__":
    client = DigestorClient()
    client.get_digest("This is a test")
