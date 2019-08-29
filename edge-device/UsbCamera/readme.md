AI Video Camera Server
=================================

AI Video Camera Server is a sample Windows-only application that connects to USB cameras and makes their
output available as an HTTP web service that is compatible with the
[AI Video Edge Device](../docs/SpinUpEdgeDevice/readme.md) used with the AI Video Solution Accelerator.

### Limitations
The AI Video Camera Server sample code and does not include features needed for production 
including:
* **Transport security:** The AI Video Camera Server serves images over HTTP, but
production servers will need mutually authenticated TLS. This feature is easy to 
add.
* **Error recovery:** As sample code, AI Video Camera Server does only minimal error checking.
* **Linux support:** Production scenarios will almost always have Camera Server equivalents
running on Linux, but the AI Video Camera Server is currently Windows-only.

### Prerequisites
1. Install [Visual Studio 2017](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2019) or later.

### Building
1. Clone [this github repo](https://github.com/Azure-Samples/AI-Video-Intelligence-Solution-Accelerator) in a convenient location.
1. Open Visual Studio as Administrator.
1. Open the [AI Video Camera Server solution]().
3. Click `Ctl-Shift-B` to build the solution.

### Running with camera identification
1. Open the CameraServer project properties and add `-id` to the Command Line Arguments. This argument 
will cause an image window to open for each camera. 
1. Click `F5` to run in debug. An image window will open for each attached USB camera, and the
CameraId will be labeled in the image window.
2. Note the CameraId for each camera, then close the image windows.
3. Closing the image windows will allow the web server to start.

### Running without camera identification
1. Click `Ctrl-F5` to run the server with no image windows. The web server will start immediately.

### Running without Visual Studio
After you have built CameraServer.exe, you can run it without Visual Studio. Remember to run
it "As Administrator".

### Point the Edge device's CameraModule at your CameraServer
To make your Edge device retrieve images from your CameraServer,
1. Determin your CameraServer machine identification:
    1. If the CameraServer is running on a different machine than the Edge device and the CameraServer
        machine can be pinged by name, use the CameraServer machine name.
    2. If the CameraServer is running on the same machine as the Edge device or the CameraServer
        cannot be pinged by machine name, use the CameraServer's IP address
        (for example 192.168.10.54).
2. Use the Azure Portal to modify
the Module Twin for CameraModule so there is a camera entry that looks like this:
```
        "cam003": {
          "port": "http://MY_SERVER:8080/image/700",
          "id": "real_camera",
          "type": "http",
          "secondsBetweenImages": 10,
          "disabled": false
        },
```
3. Replace the `MY_SERVER` component with the CameraServer machine identification from Step 1.
4. Replace the `700` component with the camera ID you chose earlier.

### (Optional) Use CameraServer to capture images for use in training

1. Use the Azure Portal to modify
the Module Twin for CameraModule so there is a camera entry that looks like this:
```
        "cam004": {
          "port": "http://MY_SERVER:8080/image/700",
          "id": "training_images_camera",
          "type": "http",
          "secondsBetweenImages": 1,
          "skipMLProcessing": true,
          "disabled": false
        }
```
The `skipMLProcessing` parameter will cause the ProcessorModule to upload the image
to BLOB storage but skip the image analysis. No IoT Hub messages will be generated for
images from this camera.
