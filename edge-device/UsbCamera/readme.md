AI Video Camera Server
=================================

AI Video Camera Server is a sample Windows-only application that connects to USB cameras and makes their
output available as an HTTP web service that is compatible with the
[AI Video Edge Device](../docs/SpinUpEdgeDevice/readme.md) used with the AI Video Solution Accelerator.

### Limitations
The AI Video Camera Server sample code and does not include features needed for production 
including:
* **Transport security:** The AI Video Camera Server serves images via gRPC over HTTP, but
production servers will need mutually authenticated TLS. This feature is easy to 
add to gRPC.
* **Error recovery:** As sample code, AI Video Camera Server does only minimal error checking.
* **Linux support:** Production scenarios will almost always have Camera Server equivalents
running on Linux, but the AI Video Camera Server is currently Windows-only.

### Prerequisites
1. Install [Visual Studio 2017](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2019) or later.

### Building
1. Clone [this github repo](https://github.com/Azure-Samples/AI-Video-Intelligence-Solution-Accelerator) in a convenient location.
1. Open the [AI Video Camera Server solution]() in Visual Studio.
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

