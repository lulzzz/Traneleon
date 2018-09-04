﻿# WEBFLOW

[![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/v/ritwickdey.LiveServer.svg?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=ackara.webflow)
[![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/d/ritwickdey.LiveServer.svg?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=acklann.webflow)
[![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/r/ritwickdey.LiveServer.svg?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=acklann.webflow)
---

## What is WebFlow
Webflow is a Visual Studio extension that automates the bundling and optimization of your web assets (`.ts`, `.sass` `images`, etc). It is a convenient alternative to task-runner scripts; stop maintaining code that do not provide value to your customer.

## How does it works
Webflow works by utilizing a configuration file that enables and customize the input-output of it operations.

```xml
<project xmlns="https://raw.githubusercontent.com/Ackara/WebFlow/master/src/WebFlow/intellisense.xsd">
    <images>
        <optimize compression="lossless" progressive="true">
            <pattern>wwwroot/**/*.jpg</pattern>
        </optimize>
    </images>
    
    <!-- Uses Defaults -->
    <sass />
    <typescript />
</project>
```
When opening a web-solution the configuration file will be added automatically if missing. To disable this feature you can visit the options page. `Tools > WebFlow`
![settings](art/screenshots/options.png)

As long as a configuration file exist, your files will be processed automatically after being saved. You can disable "transpile files when saved" feature by un-checking the 'watch' icon in the solution-explorer.

![watch button](art/screenshots/onsave.png)

To manually transpile your files, you can select then right-click on any file or folder within the solution-explorer and click "Transpile".

![transpile button](art/screenshots/item.png)

## Where can I get it
Webflow can be install from the [marketplace](https://marketplace.visualstudio.com/items?itemName=acklann.webflow), [nuget](https://www.nuget.org/packages/Acklann.webflow/) and [powershellgallery](https://www.powershellgallery.com/packages/webflow/)

`PM> Install-Package Acklann.Webflow`
