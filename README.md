# MlNet
This repo has an example of using ML.NET (v0.11.0) in a multitude of ways. Open the main SLN file using VS 2017.

ML.NET is a great way for .NET developers to get thier hands dirty with machine learning (ML) in general, but to use smaller models (not deep learning) that can be easily consumed in various ways. 

## MLNetWrapper
Contains wrapper classes on top of ML.NET to simplify the ability to 

* Train a model
* Create a model
* Save a model to either local disk or Azure Blob Storage
* Execute a model stored on either the local disk or Azure Blob Storage

Further, it extends the functionality to catpure scored records with the results to create another training file that can be used to verify the results, and append to the existing training file. This step makes retraining simpler. 

## FactoryModel
This is a similar example that I've used for years that has a  contrived data set from a "factory" floor. The devices report up 5 features and there is one label - state - that identifies if the machine is OK (1) or about to break down (0).

This model is built on top of MLNetWrapper.

## SentimentModel
This model was taken from one of the excellent existing [examples](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis) examples from the ML.NET team.

Thier solution is then wrapped with MLNetWrapper.

## LocalApplication
This shows how to use both of the models named above. In it:

* Models are trained
* Models are evaluated
* Models are saved to local disk AND Azure Blob Storage
* Models are executed against single and multiple records.

## ServerlessFunction
This wraps the FactoryModel in an Azure Function that can be depoloyed to Azure.

## WebApi
This wraps the FactoryModel in an Azure WebApi with the ability to build a Docker container with it and then deploy that to an Azure Container Instance (ACR), Azure Kubernetes Service (AKS), or a local docker instance.

***NOTE*** : Deploying the Docker image is NOT included in the instructions, though finding out how to do so is very straightforward with the right web search. 