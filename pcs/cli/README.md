AI Video Intelligence CLI
==========================

Command Line Interface for deploying a modified version of 
[Azure IoT Solution Accelerator](https://www.azureiotsolutions.com) into a
user's Azure subscription.

### How to use the CLI

[Instructions for using this modified CLI are here.](../../readme.md#deploy-the-ai-video-intelligence-solution-accelerator)
   

### Customizing

This version of the CLI only supports basic C# deployment.

To publish an updated version of the CLI, 
1. Update the [version in index.ts](src/index.ts#L359).
2. Update the `pcsReleaseVersion` in 
    [basic.json here](solutions/remotemonitoring/armtemplates/basic.json#L377) and `pcsDockerTag` in
     and [basic.json here](solutions/remotemonitoring/armtemplates/basic.json#L382).
3. Publish your changes to master, then tag the commit with your release version.
