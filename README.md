# WebDownloader

Project Description
-------------------

A lightweight extension to WebClient that enables support for timeouts and synchronous downloads.

### Current Features
* Timeout support for downloading files: Configurable timeout that prevents web client to stuck in endless download process.
* Synchronous downloading files: Thred will now wait until download is completed or any download error occur. Evnets are still available to use.

### To do list
* Add support for extended features for uploading files.
* Rewrite timer object to be passed into events insted of being class-wide available.
* Add tests and examples.
* Create nuget package.