from concurrent import futures
import cv2
import grpc
import numpy as np
import time

from client import PredictionClient
import fpgagrpc_pb2
import fpgagrpc_pb2_grpc
import ssdvgg_utils

# Tensor outputs for retail model
tensor_outputs = [
    'ssd_300_vgg/block4_box/Reshape_1:0',
    'ssd_300_vgg/block7_box/Reshape_1:0',
    'ssd_300_vgg/block8_box/Reshape_1:0',
    'ssd_300_vgg/block9_box/Reshape_1:0',
    'ssd_300_vgg/block10_box/Reshape_1:0',
    'ssd_300_vgg/block11_box/Reshape_1:0',
    'ssd_300_vgg/block4_box/Reshape:0',
    'ssd_300_vgg/block7_box/Reshape:0',
    'ssd_300_vgg/block8_box/Reshape:0',
    'ssd_300_vgg/block9_box/Reshape:0',
    'ssd_300_vgg/block10_box/Reshape:0',
    'ssd_300_vgg/block11_box/Reshape:0'
]

client = PredictionClient('10.121.114.46', 50051)

class ImageServicer(fpgagrpc_pb2_grpc.FpgaGrpcChannelServicer):
    """
    gRPC server for handling images to be submitted to FPGA model.
    """
    def __init__(self):
        self.port = fpgagrpc_pb2.VideoProcessorToFpgaPort

    def SubmitImage(self, request, context):
        """
        Implementation of the SubmitImage method described in the .proto file.
        """
        image = request.image
        #TODO: finish this function!!!
        #TODO: do stuff with the image and return the response
        img = cv2.imdecode(image, cv2.IMREAD_COLOR)
        # image is alread sized to 300x300 so we don't need to resize
        img = img[:, :, ::-1]
        img = img - (123, 117, 104)
        img = np.asarray(img, dtype=np.float32)
        img = np.expand_dims(img, axis=0)
        result = client.score_numpy_arrays({'brainwave_ssd_vgg_1_Version_0.1_input_1:0':img},
                                           outputs=tensor_outputs)
        classes, scores, bboxes = ssdvgg_utils.postprocess(result, select_threshold=0.5)

    def start_server(self):
        """
        Start the server and prepare it for servicing incoming connections.
        """
        server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        fpgagrpc_pb2_grpc.add_FpgaGrpcChannelServicer_to_server(ImageServicer(),
                                                                server)

        server.add_insecure_port('[::]:{}'.format(self.port))

        server.start()
        print("FPGA GRPC server started.")
