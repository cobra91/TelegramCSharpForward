TelegramCSharpForward
-------------------------------

_Unofficial_ Telegram (http://telegram.org) client library implemented in C#.

It is an ideal solution for any developer who wants to send data directly to Telegram users or write their own custom Telegram client.

There is a Program class which allows you to <b>redirect all the channels you want to a particular channel</b>.

This has been designed to be able to correct the price differences between Axitrader and Vantage / FXCM brokers but in the end it is a simple "CalculOffset (...)" function that you can delete if you are not interested.

The message history is logged in a database (SQLite) in order to be able to manage the Response functionalities because it seems to me that we cannot retrieve the history of a channel for which you are not the administrator.

:star2: If you :heart: library, please star it! :star2:

# Table of contents

- [How do I add this to my project?](#how-do-i-add-this-to-my-project)
- [Starter Guide](#starter-guide)
  - [Quick configuration](#quick-configuration)
  - [First requests](#first-requests)
- [Contributors](#contributors)
- [FAQ](#faq)
- [Donations](#donations)
- [Support](#support)
- [License](#license)

# How do I add this to my project?

Build from source

1. Clone TLSharp from GitHub
1. Compile source with 2019

# Starter Guide

## Quick Configuration
Telegram API isn't that easy to start. You need to do some configuration first.

1. Create a [developer account](https://my.telegram.org/) in Telegram. 
1. Goto [API development tools](https://my.telegram.org/apps) and copy **API_ID** and **API_HASH** from your account. You'll need it later.

## First requests
To start work, edit the file app.config

```text 
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
        <add key="apidId" value="YourApiId" />
        <add key="ApiHash" value="YourApiHash" />
        <add key="PhoneNumber" value="YourPhoneNumberWithAreaCode" />
        <add key="myChanId" value="YourChanIdToRedirectAllMessage" />
    </appSettings>
</configuration>
```
When user is authenticated, TLSharp creates special file called TLSharp.dat into C:\Users\<b>YourUsername</b>\AppData\Roaming\TLSharp. 

In this file TLSharp store all information needed for user session. 

So you need to authenticate user every time the TLSharp.dat file is corrupted or removed.

In this directory, you will also find the SQLite database which will be created on the first launch with an error message but if you restart the application then it will work (known bug to be corrected)

# FAQ

#### What API layer is supported?
The latest layer supported by TLSharp is 66. If you need a higher layer, help us test the preview version of (your feedback is welcome!)

#### I get an error on first launch !

The SQLite database which will be created on the first launch with an error message but if you restart the application then it will work (known bug to be corrected)

**Attach following information**:

* Full problem description and exception message
* Stack-trace
* Your code that runs in to this exception

Without information listen above your issue will be closed. 

# Donations
Thanks for donations! It's highly appreciated. 
<a href="https://www.paypal.com/donate?hosted_button_id=QZWT9BW3BDEY2" title="Support project"><img src="https://www.paypalobjects.com/en_US/FR/i/btn/btn_donateCC_LG.gif"></a>

List of donators:
* 

# Support
If you have troubles while using TLSharp, I can help you for an additional fee. 

My pricing is **100$/hour**. I accept PayPal. To request a paid support write me at Telegram @Cobra91310, start your message with phrase [PAID SUPPORT].

# Contributors
* 

# License

Created from the TLSharp library of sochix https://github.com/sochix/TLSharp

**Please, provide link to an author when you using library**

The MIT License

Copyright (c) 2020 Damien LECOMTE

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
