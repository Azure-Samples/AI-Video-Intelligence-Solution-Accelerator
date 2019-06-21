# AI Video Deployment

### Prerequisites
* Microsoft Azure subscription
* [npm version 6.4.1 or greater](https://www.npmjs.com/get-npm)
* [nodejs version 10.14.2 or greater](https://nodejs.org)

## Deploy the AI Video Intelligence Solution Accelerator

1. Clone the 
[AI Video Intelligence Solution Accelerator](https://github.com/Azure-Samples/AI-Video-Intelligence-Solution-Accelerator)
(this repo).
1. From a command prompt: 
   * navigate to `AI-Video-Intelligence-Solution-Accelerator/pcs/cli`
   * `npm uninstall -g iot-solutions` (remove any other versions)
   * `npm ci`
   * `npm start`
   * `npm link` 
   * `pcs login` (you will be prompted for your subscription credentials)
   * `pcs -t remotemonitoring -s basic -r dotnet` (no other options are supported for the AI Video
Intelligence Solution Accelerator)
1. Wait for deployment to complete
1. Verify that deployment completed successfully.  At the end you should have a statement something like:</br> `Solution: MyDeployment is deployed at https://MyDeployment.azurewebsites.net`
1. While signed into the same account that was used in the `pcs login` step, launch the Remote Monitoring 
website in a browser.  Verify that the site loads.
1. This sample does not use the standard solution accelerator simulated devices, so to reduce costs, 
click the settings gear icon in the top right corner, make sure **Simulation Data** is turned off.

#### Enable access to BLOB storage for image storage and retrieval

1. Login to https://portal.azure.com with the same account you used to deploy the CLI
1. Select the Resource Group to which the Remote Monitoring Template was deployed
1. Select the Storage Account
1. Under **Settings**, select **Configuration**
1. Disable **Secure transfer required** and **Save**
1. Under **Blob Service**, select **Blobs**
1. Create a new Container called **still-images**, set **Public access level** to **Private**
1. After **still-images** is created, click the **...** at the end of its row, select **Access policy**
1. Add a policy with the identifier **allowWebUiDirectAccess** and **Read** permissions, click **OK** 
and be sure to click **Save**

#### To give other users access to the solution website
1. On https://portal.azure.com go to the **Azure Active Directory** resource and select **Enterprise applications**.
1. Change the selection in the **Application Type** dropdown to **All Applications** and click **Apply**.
1. Find the deployment you are attempting to administer and select it. Searching by deployment name (e.g. MyDeployment) using the text box right below the **Application Type** dropdown might be helpful.
1. Click **Users and groups** from the **Manage** section of the vertical menu choices.
1. Select **Add user** from the button options above the list of current users.
1. In the **Add Assignment** panel that appears select **User**
1. In the subsequent dialog, either select a user who is already a part of your organization or invite a new user with the *Search* box, and click **Select**
1. Back in the **Add Assignment** panel **Select Role**
1. Choose either **Admin** or **Read Only** as appropriate and click **Select**
1. Finally click **Assign**

## AI Video Edge Device Deployment

In order to see image data
in the solution web site you'll need to create at least one AI Video Edge Device. 
AI Video Devices for the AI Video Intelligence Solution Accelerator are connected to 
real or simulated cameras which collect the images to be processed. 

If you create your AI Video Edge Device locally (rather than as a cloud resource) then
you may optionally connect it to a USB camera using the simple
[Camera Server sample application](edge-device/UsbCamera/readme.md).

#### Choose an Azure AI Video Device platform
The primary platform for Azure IoT Devices in the AI Video Intelligence Solution Accelerator is
Azure Data Box Edge, but non-DBE computers can also be used. Four common options
for hosting the Azure IoT Device are shown below. This walkthrough follows the fourth option, 
**Deploy to an Azure cloud VM**.
*  **Deploy to a Data Box Edge:** If you are deploying to a Databox Edge as a device, you will 
need to connect it to the IoT Hub that was created when you ran the AI Video 
Intelligence Solution Accelerator. 
    1. Follow the 
       [Data Box Edge instructions](https://docs.microsoft.com/en-us/azure/databox-online/data-box-edge-deploy-configure-compute-advanced) 
       to create a Data Box Edge device that is connected to your IoT Hub. 
    2. In the Azure Portal, navigate to the DBE device that you just created and 
       under **Edge Compute** select **Modules**.
    2. Skip down this page to [Populate the Device with the Required Modules](#populate-the-device-with-the-required-modules) to complete the deployment.
* **Run an Azure IoT Device in VS Code:** 
    1. Using the instructions in 
    [this article](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module),
    set up VS Code and add all the listed prerequisites.
    2. Navigate to [the Azure IoT Device source code](edge-device/AIVideoEdgeDevice/EdgeDevice/readme.md)
    and open that folder (which contains the `deployment.template.json` file) in VS Code.
    2. Register a new Azure IoT Edge device from Visual Studio Code using 
    [these instructions](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device-vscode).
    3. [Build and run the solution](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module#build-and-run-container-for-debugging-and-debug-in-attach-mode).
      The solution will not work correctly the first time you run it, but you must run it once
      before performing the next step.
    4. Set the various Module Twin properties as described
    below in [Populate the Device with the Required Modules](#populate-the-device-with-the-required-modules).
    Now that the module twin properties are set correctly the solution will run as designed.
* **Deploy to a local Linux machine:** 
    1. Perform the [Register a new Edge Device with the IoT Edge Hub](#Register-a-new-Edge-Device-with-the-IoT-Edge-Hub) step just below this section.
    1. Set up your Linux machine and provide the connection string according to these instructions:
      [Install the Azure IoT Edge runtime on Linux (x64)](https://docs.microsoft.com/bs-latn-ba/azure/iot-edge/how-to-install-iot-edge-linux).
    2. Skip down this page to [Populate the Device with the Required Modules](#populate-the-device-with-the-required-modules) to complete the deployment.
* **Deploy to an Azure cloud VM:** Continue this walkthrough to deploy to an Azure VM.

## Register a new Edge Device with the IoT Edge Hub
1. In the Azure Portal open the the IoT Hub that was created by the solution accelerator.
1. Under **Automatic Device Management**, select **IoT Edge**
1. Near the top of the page, select **Add an IoT Edge device**
1. Provide a name in the **Device ID** section, then click **Save**. This will create a new 
   device registration in your IoT Hub.
1. Click on the new device in the device list and copy the device connection string
   (primary key) from the device details page. Save this for later use.
1. Notice the **Modules** list at the bottom of this page. This is where we perform the next step
    so leave this tab open in your browser.

## Run the Edge Device on an Azure cloud VM
1. Open [portal.azure.com](https://portal.azure.com) 
2. Add a new *Azure IoT Edge on Ubuntu* resource to the resource group containing your solution.
3. Enter a name for your VM.
3. Under **Administrator account** select "password" as the Authentication Type.
4. Under **Inbound port rules** select "SSH".
3. Click "Review + create".
6. Wait for the deployment to finish.
8. Click the "Go to resource" button.
9. Open the **Run command** option.
10. Select the **Run shell script** item.
11. Enter `/etc/iotedge/configedge.sh "{device_connection_string}"`, where
    `{device_connection_string}` is the connection string you saved in
    [Register a new Edge Device with the IoT Edge Hub](#register-a-new-edge-device-with-the-iot-edge-hub). This process takes several minutes and gives no feedback until it completes,
    so be patient.

## Populate the Device with the Required Modules
#### Add `grocerymodel`
1. Return to your browser tab with the device's **Modules** list. Now we're ready to add our 
  modules: grocerymodel, CameraModule, and VideoProcessorModule.
1. Select **Set modules** at the top of the page.
1. In the device details in the Azure Portal, in the lower section of the 
**Set modules** page, under **Deployment Modules**, click on **Add**.
1. From the drop-down menu select **IoT Edge Module**. You will be asked for the name and the address of the module being added.
1. Name the module "grocerymodel".  Note that case matters on the module names.
1. Use `docker.io/azureaivideo/grocerymodelquiet:0.0.1` as the module URL.
1. There is no module twin data for this module, so leave the checkbox unchecked.  Click **Save**

#### Add `VideoProcessorModule`
1. Click **Add** to add another IoT Edge Module. This time we'll add the *videoprocessormodule*, 
naming it "VideoProcessorModule". Provide the URI of the module as described above. 
The URI is: `docker.io/azureaivideo/videoprocessormodule:0.0.24-amd64`
1. Generate a SAS Url for your container:
    * Go to the storage account
    * Under **Settings**, Select the **Shared Access Signature**
    * On **Allowed services** select **Blob only**
    * On **Allowed resource types** select **Container + Object**
    * On **Allowed permissions** select **Write + Create**
    * Leave **Start time** as-is
    * Select some appropriate **End time**
    * Click the **Generate SAS and connection string** button, use the generated connection string for the contents of blobStorageSasUrl in the next step
1. Modify the module twin. The module twin should look like
```json
{
  "properties.desired": {
    "blobStorageSasUrl": "BlobEndpoint=https://MYSTORE.blob.core.windows.net/;QueueEndpoint=https://MYSTORE.queue.core.windows.net/;FileEndpoint=https://MYSTORE.file.core.windows.net/;TableEndpoint=https://MYSTORE.table.core.windows.net/;SharedAccessSignature=sv=2018-03-28&ss=b&srt=co&sp=wc&se=2020-04-05T05:00:34Z&st=2019-04-20T21:00:34Z&spr=https&sig=BAjwrvjBRMSxN8iRRrcB6g5B0zvh4MxsmLF%2BpE1rBeE8%3D",
    "uploadThreshold": 5
  }
}
```
5. You can adjust *uploadThreshold* adjust how often the processor will upload an image if no recognition 
is found in the images.  1 means all unrecognized images will be uploaded.  5 means 1 out of every 5 unrecognized 
images will be uploaded.
1. Click **Save**.

#### Add `CameraModule`
1. Click **Add** to add another IoT Edge Module. .
1. From the drop-down menu select **IoT Edge Module**. You will be asked for the name and the address of the module being added.
1. Name the module "CameraModule".  Note that case matters on the module names.
1. Use `docker.io/azureaivideo/cameramodule:0.0.15-amd64` as the module URL.
1. For the camera module, the module twin must be configured. Click the checkbox for **Set module twin's desired properties**. Each device will have different configuration values for the camera module. Here is an example module twin for a camera module configured for two cameras:
```json
{
  "properties.desired": {
    "cameras": {
      "cam01": {
        "port": "cycle-0",
        "id": "Camera 01",
        "type": "simulator"
      },
      "cam02": {
        "port": "counter",
        "id": "Camera 02",
        "type": "simulator"
      }
    }
  }
}
```
The `cam01` simulated camera will cycle through simulated images named `cycle-0-0` through `cycle-0-5`,
and the `cam02` simulated camera will repeatedly send a single image named `counter`.

6. Click **Save**

#### Finish the module deployment
1. Click **Next** to advance to page 2, **Specify Routes**. There are no required changes here, so 
2. click **Next** to advance to page 3, **Review Deployment**. 
3. Click **Submit**.
4. Return to the **Device Details** page and wait for all of the modules in your device to show
    a status of "running". This process takes several minutes. One of the transient states 
    you may see is "back off"; this does not indicate a problem.


## View the results

You can now open the web site for your deployment and select one of the cameras from the dropdown.
Images from your new device should appear in the "Insights from video stream" window.

#### Optional: connect a real camera
If you deployed your AI Video Device locally, you can connect it to a USB camera using the simple
[Camera Server sample application](edge-device/UsbCamera/readme.md).


