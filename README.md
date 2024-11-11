# TEMPLATE
[![standard-readme compliant](https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square)](https://github.com/RichardLitt/standard-readme)

## Table of Contents
- [TEMPLATE](#L-TEMPLATE)
  - [Table of Contents](#table-of-contents)
  - [Security](#security)
  - [Background](#background)
  - [Install](#install)
  - [Usage](#usage)
  - [API](#api)
  - [Testing](#testing)
  - [Versioning](#versioning)

## Security
How is the API secured?

## Background
Give a brief synopsis of the API.

## Install
What are the install steps required?

## Usage
How do you use the API?

## API

## Testing
How do you test the API?

## Versioning
Does the API use a versioning strategy?

## git hooks
We use husky.net to execute any git hooks that make sense for CloudPrint.  Generally we want to execute tasks locally before finding they are problems on the server.  To this end run `dotnet tool install Husky` to ensure husky is installed locally and the csharpier formatter will run before each commit to ensure the formatting has been applied locally.  In the event csharpier has not been run before the commit the local commit will fail.

If this happens run `dotnet csharpier .` which will run against all files in the current directory, and move through the commit process again.