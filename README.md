# Overview

Welcome to the Seeq Connector SDK for C#!

This SDK is intended for developers that wish to write a Seeq datasource connector that can be loaded by a Seeq agent
and facilitate access to data in Seeq.

Seeq connectors can be written in Java or C# but this repository is intended to be used for developing C# Connectors. 
C# development requires Windows operating system.

It is recommended that you initially test with a "test" version of your Seeq Remote Agent. This will seperate your 
production connections from your test connections, allowing you to restart the remote agent without impacting users. 
This repository contains an embedded remote agent that allows your development environment to interactively debug 
your connector. 

# Environment Setup

## The Build Environment

The C# version of the SDK depends upon Microsoft Visual Studio for building and debugging. This SDK is tested with
Microsoft Visual Studio 2017 - 2022.

The C# SDK also depends on .NET version 4.8. When upgrading .NET, you may need to restart your machine for the new
version to take effect.

To begin using the SDK:

1. Launch the Windows Command Prompt.
1. Change directory to the root directory of this SDK.
1. At the prompt, execute the *environment script*: Type `environment`.

Throughout this document, we will refer to the *build environment*, which is simply a command prompt or terminal window
where you've executed the environment script as described.

## Verifying your Environment

Before doing anything else, we recommend that you build the connector template and ensure that it is fully working with
your private system.

From the root directory, execute the `build` command. This command will download dependencies from the web, so make
sure you have a good internet connection. If you have any non-obvious issues building this project, please post your
issue along with any error messages on the [Seeq Developer Club forum](https://www.seeq.org/forum/25-seeq-developer-club/). 

From your build environment, execute the `ide` command. This command will launch Microsoft Visual Studio (VS), which you
will use for development and debugging.

Take the following steps to confirm a properly configured development environment:

1. Once VS is finished loading, right click on the `Seeq.Link.SDK.Debugging.Agent` project in *Solution Explorer* and
   select *Set as Startup Project*.
1. In the solution configuration dropdown, select 'Debug'. **This step is required to ensure the necessary files are
   built for debugging.**
1. Select *Build* > *Build Solution* from the main menu.
1. Wait for building to finish.
1. Confirm that no compile errors occurred.
1. Open the `EntryPoint.cs` file in the `Seeq.Link.SDK.Debugging.Agent` project.
1. Modify the URL on the line `const string seeqHostUrl = "https://yourserver.seeq.host"` to match your Seeq server
1. Retrieve the agent_api_key from your Seeq Server by logging in as a Seeq Administrator and navigating to the API
   Documentation page. Expand the System group and expand GET /system/agentKey. Click Execute
1. Modify the `agent_api_key` in `resources\data\keys\agent.key` by replacing the `<your_agent_api_key>`
   with the key that is located in the top response from the previous step. Note: it should only include the value. For
   example if the return was `{"agentKey": "superSecret123"}` then the key is `superSecret123`
1. Set a breakpoint (*Debug* > *Toggle Breakpoint*) on the first line of the `Main()` function.
1. Select *Debug* > *Start Debugging* to launch the debugger.
1. You should hit the breakpoint you set. **This verifies that VS built your project correctly and can launch it in its
    debugger.**
1. With execution paused at the breakpoint, open the `MyConnector.cs` file in the
    `MyCompany.Seeq.Link.Connector.MyConnector` and put a breakpoint on the first line of the `Initialize()` function.
    Because the connector is loaded dynamically, this breakpoint may appear broken until it is actually hit.
1. Click *Debug* > *Continue*. You should hit the next breakpoint. **This verifies that the debugging agent can load
    the template connector correctly.**
1. Click *Debug* > *Delete All Breakpoints* and then *Debug* > *Continue*.
1. Bring up Seeq Workbench and click on the connections section at the top of the screen. You should
    see `My Connector Type: My First Connection` in the list of connections, with 5000 items indexed.
1. In Seeq Workbench's *Data* tab, search for `simulated`.
1. A list of simulated signals should appear in the results. Click on any of the results.
1. The signal should be added to the *Details* pane and a repeating waveform should be shown in the trend. **This
    verifies that the template connector is able to index its signals and respond to data queries.**

Now you're ready to start development!

Troubleshooting hint: Within the connector project in the SDK folder, there should be a *bin/Debug* folder containing
a `.pdb` file and a `.dll` file as a result of building the solution. If these files are not present, it will not be
possible to debug the connector.

## Developing your Connector

We recommend that you just modify the template connector directly. This shields you from having to recreate all of the
configuration that is required to correctly build and debug a new project. Visual Studio has excellent
renaming/refactoring features that make it easy. For example, you can click on any item in VS's *Solution Explorer*
and press *F2* to change it to something appropriate for your company and this particular connector.

Once you are ready to start developing, just open the `MyConnector.cs` and `MyConnection.cs` files in VS and start
reading through the heavily-annotated source code. The template connector uses a small class called
`DatasourceSimulator`. You'll know you've removed all of the template-specific code when you can delete this file from
the project and still build without errors.

Any log messages you create using the `Log` property on `ConnectorServiceV2` and `DatasourceConnectionServiceV2` will go
to the console window and to the `csharp/Seeq.Link.SDK.Debugging.Agent/bin/Debug/log/net-debugging-agent.log` file.

## Deploying your Connector

When you are ready to deploy your connector to a production environment, execute the `package` command. A zip file will
be created in the `packages` folder.

1. Shut down the Seeq Remote Agent - execute `seeq stop` in the Seeq CLI`
1. Copy the generated zip file to the `plugins/connectors` folder within Seeq's `data` folder (The data folder 
   is usually `C:\ProgramData\Seeq\data`)
1. Extract the contents of the zip file.
1. Start the Seeq Remote Agent - execute `seeq start` in the Seeq CLI

You should see your connector show up in Seeq when you go to add a datasource in the Seeq Administration Panel and you choose
your remote agent.

Once deployed, log messages you create using the `Log` property on `ConnectorServiceV2` and
`DatasourceConnectionServiceV2` will go to `log\net-link.log` file in the Seeq data folder.

## CPU Architecture

If you use the template connector project, your connector DLL will be compiled using the `Any CPU` architecture, which
means it can be loaded by a 64-bit or 32-bit .NET Agent. If you have a dependency on an architecture-specific library
(most often that means the library is `x86`) you may have to change the _platform_ of your connector DLL to be `x86`
(or `x64` in rare cases). If you do that, then your connector can only be loaded by a 32-bit .NET Agent, which means it
will have to be a _Remote Agent_. Contact Seeq Support for more information.