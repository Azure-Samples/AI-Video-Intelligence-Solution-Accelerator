# AI Video Intelligence Solution Accelerator
The AI Video Intelligence Solution Accelerator is a variant of the
[Azure IoT .NET solution for Remote Monitoring](https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet)
which uses specialized Azure IoT Edge devices to capture and process video input from cameras.

The IoT Edge devices used by this solution are capable of simulating camera input, so physical
cameras are not required to build and demonstrate the solution. The primary platform for 
the IoT Edge devices in this solution accelerator is
Azure Data Box Edge, but non-DBE computers can also be used.

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
   * `deploy-pcs.cmd` (you will be prompted for your subscription credentials)
1. Wait for deployment to complete. It takes about 10 to 20 minutes.
1. Verify that deployment completed successfully.  At the end you should have a statement something 
    like:</br> `Solution: MyDeployment is deployed at https://MyDeployment.azurewebsites.net`
1. While signed into the same account that was used in the `pcs login` step, launch the Remote Monitoring 
website in a browser.  Verify that the site loads.
1. This sample does not use the standard solution accelerator simulated devices, so to reduce costs, 
click the settings gear icon in the top right corner, make sure **Simulation Data** is turned off.

#### Enable access to BLOB storage for image storage and retrieval

1. Login to https://portal.azure.com with the same account you used to deploy the CLI
1. Select the Resource Group to which the Remote Monitoring Template was deployed
1. Select the Storage Account
1. Under **Blob Service**, select **Blobs**
1. Create a new Container called **still-images**, set **Public access level** to **Private**
1. After **still-images** is created, click the **...** at the end of its row, select **Access policy**
1. Add a policy with the identifier **allowWebUiDirectAccess** and **Read** permissions, click **OK** 
and be sure to click **Save**

#### Give other users access to the solution website
1. On https://portal.azure.com go to the **Azure Active Directory** resource and select **Enterprise applications**.
1. Change the selection in the **Application Type** dropdown to **All Applications** and click **Apply**.
1. Find the deployment you are attempting to administer and select it. Searching by deployment name (e.g. MyDeployment) 
    using the text box right below the **Application Type** dropdown might be helpful.
1. Click **Users and groups** from the **Manage** section of the vertical menu choices.
1. Select **Add user** from the button options above the list of current users.
1. In the **Add Assignment** panel that appears select **User**
1. In the subsequent dialog, either select a user who is already a part of your organization or invite a new 
    user with the *Search* box, and click **Select**
1. Back in the **Add Assignment** panel **Select Role**
1. Choose either **Admin** or **Read Only** as appropriate and click **Select**
1. Finally click **Assign**

#### Generate a BLOB Storage SAS URL
1. Open a new [portal.azure.com](https://portal.azure.com) 
    tab in your browser.
2. Navigate to the storage account for your solution's resource group.
3. Under **Settings**, Select the **Shared Access Signature**
4. On **Allowed services** select **Blob only**
5. On **Allowed resource types** select **Container + Object**
1. On **Allowed permissions** select **Write + Create**
1. Leave **Start time** as-is
1. Select some appropriate **End time** at least a few months into the future.
1. Click the **Generate SAS and connection string** button and make note
	of the generated Connection String for later use.

## AI Video Edge Device Deployment

In order to see image data
in the solution web site you'll need to create at least one AI Video Edge Device. 
AI Video Devices for the AI Video Intelligence Solution Accelerator are connected to 
real or simulated cameras which collect the images to be processed. 

If you create your AI Video Edge Device locally (rather than as a cloud resource) then
you may optionally connect it to a USB camera using the simple
[Camera Server sample application](edge-device/UsbCamera/readme.md).

### Prerequisites
* VS Code with Azure IoT Hub extensions installed according to 
[this article](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module).

### Prepare VS Code for deployment
1. Navigate to [the Azure IoT Device source code](edge-device/AIVideoEdgeDevice/EdgeDevice/readme.md)
    and open that folder (which contains the `deployment.template.json` file) in VS Code.
2. Open the `.env.template` file and save a copy of it as `.env`.
3. In the `.env` file, populate the `BLOB_STORAGE_SAS_URL` value with the BLOB Storage SAS
    Connection String that you generated earlier. Make sure the value is within quotes. 
    You won't need to add the `CONTAINER_REGISTRY_USERNAME` and `CONTAINER_REGISTRY_PASSWORD` 
    values unless you want to try modifying and
    deploying your own versions of the sample modules.

### Deploy to your chosen Azure AI Video Device platform
The primary platform for Azure IoT Devices in the AI Video Intelligence Solution Accelerator is
Azure Data Box Edge, but non-DBE computers can also be used. Here are three options for deployment:
* **Run an Azure IoT Device on your own computer using VS Code:** 
    1. Register a new Azure IoT Edge device from Visual Studio Code using 
    [these instructions](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device-vscode).
    1. Press Ctl-Shift-P and select Azure IoT Edge: Setup IoT Edge Simulator.
    3. Select the name of your newly created device.
    4. Right-click the `deployment.template.json` file and select Build and Run IoT Edge
        Solution in Simulator.

    Note that the Edge device will continue to run even if you close VS Code. To stop the Edge device,
    press Ctl-Shift-P and select Azure IoT Edge: Stop IoT Edge Simulator.

*  **Deploy to a Data Box Edge:** 
    1. Follow the 
       [Data Box Edge instructions](https://docs.microsoft.com/en-us/azure/databox-online/data-box-edge-deploy-configure-compute-advanced) 
       to create a Data Box Edge device that is connected to the IoT Hub that was created 
        when you ran the AI Video Intelligence Solution Accelerator. 
    2. In the Azure Portal, navigate to your IoT Hub.
    1. Under *Automatic Device Management*, select *IoT Edge*.
    4. In the devices list, find the device that corresponds to the DBE device you
        just connected. Its name will be the DBE name with "-edge" appended. Make note of the name.
    3. Right-click the `deployment.dbe.template.json` file and select Generate IoT Edge Deployment Manifest.
    8. In the *AZURE IOT HUB* section of VS Code, under Devices, right click the name of your DBE device.
    7. Select Create Deployment for Single Device. You will be prompted for a file name.
    8. Select the file named `deployment.dbe.amd64.json` which can be found in the `config` directory.
        Do not select the template file by mistake.
    
    The deployment to the DBE will take a few minutes, and you can monitor the progress of the deployment via the 
    Azure Portal. If the DBE's FPGA needs to be re-flashed, it may take another 10 or 15 minutes after the 
    deployment succeeds before the FPGA is flashed and operational.

* **Deploy to an Azure cloud VM:** If you don't want to run the Azure Edge device on your own computer but
    you don't have a DBE device, you can deploy to a standard Azure VM.

    1. Register a new Azure IoT Edge device from Visual Studio Code using 
    [these instructions](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device-vscode).
    1. Click on the newly created device in the device list and copy the device connection string
       (primary key) from the device details page. Save this for later use. (You may have to hit
        `F5` to see your new device in the list.)
    1. Open [portal.azure.com](https://portal.azure.com) in a new tab.
    2. Add a new *Azure IoT Edge on Ubuntu* resource to the resource group containing your solution.
    3. Enter a name for your VM.
    3. Under **Administrator account** select "password" as the Authentication Type.
    4. Under **Inbound port rules** select "SSH".
    3. Click "Review + create".
    7. After validation passes, click "Create".
    6. Wait for the deployment to finish.
    8. Click the "Go to resource" button.
    9. Open the **Run command** option under **Operations**.
    10. Select the **RunShellScript** item.
    11. Enter `/etc/iotedge/configedge.sh "{device_connection_string}"`, where
        `{device_connection_string}` is the connection string you saved in
        [Register a new Edge Device with the IoT Edge Hub](#register-a-new-edge-device-with-the-iot-edge-hub). This process takes several minutes and gives no feedback until it completes,
        so be patient.
    3. Right-click the `deployment.template.json` file and select Generate IoT Edge Deployment Manifest.
    8. In the *AZURE IOT HUB* section of VS Code, under Devices, right click the name of your device.
    7. Select Create Deployment for Single Device. You will be prompted for a file name.
    8. Select the file named `deployment.amd64.json` which can be found in the `config` directory.
        Do not select the template file by mistake.

    The deployment will take a few minutes, and you can monitor the progress of the deployment via the 
    Azure Portal.

## View the results

You can now open the web site for your deployment and select one of the cameras from the dropdown.
Images from your new device should appear in the "Insights from video stream" window.

#### Optional: connect a real camera
If you deployed your AI Video Device locally, you can connect it to a USB camera using the simple
[Camera Server sample application](edge-device/UsbCamera/readme.md).


