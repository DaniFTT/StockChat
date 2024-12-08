# StockChat Application - README

## Context:
Welcome to the StockChat application! This document explains the steps to set up and run the application, but dont worry, you will se that is very simple! 

## Pre-requisites

### Mandatory

- Docker installed on your machine.


### Optional
- Git (if you prefer to clone the repository instead of downloading the ZIP).


## Running the Application
You can choose between running the application with Visual Studio for debugging or using the command line with Docker Compose for a simpler setup.


### 1. Using Visual Studio (For Debugging)
Use this method if you want to run the application with debugging capabilities for backend services.

- Navigate to the folder where the solution is located: ``` backend/StockChat/```.
- Open the solution file (StockChat.sln) using Visual Studio.
- In the Solution Explorer panel, locate the docker-compose element.
- Right-click on docker-compose and select "Set as Startup Project". Image Below:

<img src="https://github.com/user-attachments/assets/fe09a9c2-3341-4dd3-ac28-2d5dbd4ba60a" alt="image" width="800"/>

- Ensure Docker is running on your machine. If it's not, Visual Studio will prompt you to start Docker.
- Click the Run button in the header of Visual Studio. This will build the required Docker images and start all the containers.
- Once running, the application is accessible in your browser at (is where the frontend will be hosting): ``` http://localhost:3000 ```.

### 2. Using Docker Compose in the terminal (Without Debugging)
This is a simpler method to run the application without needing Visual Studio.

- Navigate to the folder: ``` backend/StockChat/```.
- Open a terminal in this folder.
- Run the following command: ``` docker-compose up```. This will build the required Docker images and start all the containers.
- You can verify that the containers are running using Docker Desktop.
- Once running, the application is accessible in your browser at (is where the frontend will be hosting): ``` http://localhost:3000 ```.

## Additional Notes

The application uses Docker Compose to orchestrate multiple services, including:

- The backend API.
- The frontend application.
- The stock bot application.
- RabbitMQ
- SQL Server

The backend API is exposed at ``` https://localhost:8081 ```, but you should interact with the application through the frontend at ``` http://localhost:3000 ```

Enjoy using the StockChat application! 🚀
