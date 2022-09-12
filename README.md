# Coding Challenge - TvMaze Service 

[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

This project is a demo solution for the TvMaze coding challenge outlined in the TvMazeScraper.pdf found in the root of the repository.

The implementation is making use an existing AWS cloud solution template, which allows to quickly spin up AWS services to handle the scraping work and serve the results with a web api.

CI/CD is implemented using GitHub Actions. Upon commit to the "main" branch, the entire AWS infrastructure is created or updated with the latest build.

### Technologies
* .NET 6.0
* AWS CDK
* CakeBuild
* AWS SQS/SNS
* AWS DynamoDb
* AWS API Gateway

### Highlights

* [AWS CDK](https://aws.amazon.com/cdk/) framework & [CloudFormation](https://aws.amazon.com/cloudformation/) is used as the Infrastructure as Code (IaC) tool to be able to create relevant infrastructure on AWS.
* [Cake Build](https://cakebuild.net/) is used as the build automation system. 
* [AWS](https://aws.amazon.com/) is the cloud platform where the solution is realised. 
  * Cloud Native (serverless) components are used such a [SNS](https://aws.amazon.com/sns/), [SQS](https://aws.amazon.com/sns/), [DynamoDB](https://aws.amazon.com/dynamodb/), [API Gateway](https://aws.amazon.com/api-gateway/).

### Urls

* API Swagger: https://tvmazeapi-dev.spark-logic.com/swagger
* API Get show by id endpoint: https://tvmazeapi-dev.spark-logic.com/api/shows/1
* API Get shows endpoint: https://tvmazeapi-dev.spark-logic.com/api/shows/getall/10
* GitHub actions with build and deploy log
https://github.com/ronfa/tv-maze-service/actions 

### Scraper
*TvMaze.com has between 64000 and 65000 shows in its database.
*We are using SNS (Simple Notification Service) for adding scrape messages to an SQS queue (Simple Queue Service). 
*We are adding 6500 messages, each with 10 shows to scrape (1-10, 11-20..), with a total of 65000. this can be configured.
*A message handler (lamdba function) is then picking up messages from the queue and performing the scraping work (each lambda run scrapes 10 shows).
The lambda function can  be scaled to finish the scrape work as needed.
With a basic setting of 200 concurrent lambda executions, the scraping was finished within 3 minutes.
*We are using the show endpoint and embedding the cast member information, this allows us to make a single api call per show. For example: https://api.tvmaze.com/shows/1?embed=cast

* The TvMaze Scraper is currently a console application which can be executed locally.
  * Url: https://github.com/ronfa/tv-maze-service/blob/main/src/CodingChallenge.Console/TVMazeConsoleRunner.cs
* The console app is adding as many scrape messages as required to a queue.
* The EventQueueProcessor is the message handler, and is in charge of making the api calls to TvMaze
* The results are stored in DynamoDb 

### API
Get show by id endpoint: https://tvmazeapi-dev.spark-logic.com/api/shows/1
Get shows endpoint: https://tvmazeapi-dev.spark-logic.com/api/shows/getall/10

* AWS API Gateway and .NET 6 Web API query the data from the same DynamoDb table that is used for persisting the scraped data.
* Paging is forward-only. Meaning you will receive a pagination token with each response, which allows you get the next set of results. 

### CI/CD using GitHub Actions
To simulate CI/CD pipeline, we are using GitHub Actions.
Upon PR or commit on the "main" branch, the full pipeline triggers, which is using the /build/ project to build and deploy all the necessary services to AWS.

You can see a full log of the last deploying by clicking on "Actions" tab on top and then on the last commit.
https://github.com/ronfa/tv-maze-service/actions 

### Deploying the project in your own AWS account
If you wish to deploy this project on your AWS account, follow the steps to create an AWS account as well as setting up credentials and named profile:

* Sign up to AWS and create IAM user account and credentials https://docs.aws.amazon.com/cli/latest/userguide/getting-started-prereqs.html
* Install the AWS CLI, directions can be found here: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html
* Configure a named profile, with the name "default" holding your credentials https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-profiles.html
* You can now build and deploy from your machine directly to AWS by executing build.sh or build.ps1 found in the build folder.
* It is recommended to setup your own GitHub repository and use GitHub Actions to deploy the solution for you. see next section for details.

### Using GitHub Actions for build and deployment
* GitHub is using the yaml file location in the root of the repo under /.git/workflows/cakebuild.yml file
Some changes are needed there, namely the AWS account id, the region, the certificate ARN if you wish to use your own domain name.
* The metadata.xml file in the root of the repo contains the system/subsystem/platform to use, change it to your naming convention.
* Add secretes on GitHub to store your AWS credentials.
You do this by going to your repository, clicking settings on the top menu, and then "Secrets" and "Actions" on the left menu. add 2 secrets and set their values properly:
  * AWS_ACCESS_KEY_ID
  * AWS_SECRET_ACCESS_KEY
* Make a commit on your repo for "main" branch, and your application should get deployed to AWS under your account.

### Build

* In this particular cake build project, the purpose of the build pipeline is to produce artifacts which can be deployed to desired platform e.g. AWS.
* The buid pipeline makes sure:
  * A version is determined for the specific version of the code based on git tags & commits
  * Dotnet code is built and all the tests passes.
  * A docker image is created and pushed to private Docker Registry (ECR).
  * "./build/build.sh --target=build" command triggers the build pipeline.

### Deploy

* In this particular cake build project, the purpose of the deploy pipeline is to create relevant serverless infrastructure using CloudFormation if they do not exist and create/update lambda function(s) with the latest corresponding docker images. 
* This pipeline is created with the Continuous Deployment in mind. For Continuous Delivery, a separate deployment artifact can be generated.
* "./build/build.sh --target=deploy" command triggers the deploy pipeline.

### AWS CDK

* This solution contains a single cdk project which contains definitions for 4 stacks. The stacks can also be defined in separate projects but this setup is chosen to showcase the multi-stack support capability.
* Infra Stack must be deployed first. 
* Database Stack is deployed after infra stack. 
* Main/Application Stack is then deployed as it has dependencies on the Infra & Database stacks.
* API stack is then deployed as it has dependencies on the Infra & Database stacks
