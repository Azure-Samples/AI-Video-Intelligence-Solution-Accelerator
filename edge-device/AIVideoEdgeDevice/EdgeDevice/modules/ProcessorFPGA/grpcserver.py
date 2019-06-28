# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

from concurrent import futures
import cv2
import grpc
import numpy as np
import time

import traceback

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

client = PredictionClient("grocerymodelfpga", 50051)

class ImageServicer(fpgagrpc_pb2_grpc.FpgaGrpcChannelServicer):
    """
    gRPC server for handling images to be submitted to FPGA model.
    """
    def __init__(self):
        self.port = fpgagrpc_pb2.VideoProcessorToFpgaPort
        self.server_port = 28962

    def SubmitImage(self, request, context):
        """
        Implementation of the SubmitImage method described in the .proto file.
        """
        print("In SubmitImage")
        rv = fpgagrpc_pb2.ImageReply()
        print("Created ImageReply for return value.")
        rv.error = ''
        print("Set reply's error to empty string.")
        try:
            print("Acquiring image")
            image = request.image
            print("Image size is {} bytes".format(len(image)))

            arr = np.asarray(bytearray(image), dtype=np.uint8)
            img = cv2.imdecode(arr, cv2.IMREAD_COLOR)
            img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            # image is already sized to 300x300 so we don't need to resize
            img = img[:, :, ::-1]
            img = img - (123, 117, 104)
            img = np.asarray(img, dtype=np.float32)
            img = np.expand_dims(img, axis=0)
            print("Scoring image")
            result = client.score_numpy_arrays({'brainwave_ssd_vgg_1_Version_0.1_input_1:0':img},
                                               outputs=tensor_outputs)
            print("Post-processing scores")
            classes, scores, bboxes = ssdvgg_utils.postprocess(result, select_threshold=0.5)
            processed_results = {}
            processed_results["classes"] = classes.tolist()
            processed_results["scores"] = scores.tolist()
            processed_results["bboxes"] = bboxes.tolist()
            print(processed_results)

            print("Building reply")
            # Put results into ImageReply object
            print("Adding classes...")
            for item in classes.tolist():
                rv.classes.append(item)
            print("Adding scores...")
            for item in scores.tolist():
                rv.scores.append(item)
            print("Adding bounding boxes...")
            for bb in bboxes.tolist():
                bbox = fpgagrpc_pb2.BoundingBox()
                bbox.yMin = bb[0]
                bbox.xMin = bb[1]
                bbox.yMax = bb[2]
                bbox.xMax = bb[3]
                rv.boxes.append(bbox)
        except Exception as ex:
            print("Unexpected error:", ex)
            traceback.print_exc()
            rv.error = "Unexpected error: {}".format(ex)

        print("Returning from SubmitImage")
        return rv

    def serve(self):
        """
        Start the server and prepare it for servicing incoming connections.
        """
        server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        fpgagrpc_pb2_grpc.add_FpgaGrpcChannelServicer_to_server(ImageServicer(),
                                                                server)

        print("using port {}')".format(self.port))
        server.add_insecure_port('[::]:{}'.format(self.port))

        server.start()
        print("FPGA GRPC server started.")

        try:
            while True:
                time.sleep(60*60*24)

        except KeyboardInterrupt:
            print ( "FPGA GRPC server stopped" )
