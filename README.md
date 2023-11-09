# Bulky_MVC
Bulky_MVC is a web application designed for bulk book purchases, featuring separate admin and customer areas.
## Table of Contents
1. Introduction
2. Getting Started
- Installation
- Configuration
3. Usage
4. Project Structure
5. Contributing

  
## Introduction
Bulky_MVC is a web application built on the MVC (Model-View-Controller) architecture.
The platform facilitates bulk book purchases with dedicated admin and customer areas,
providing a seamless experience for both user types.


## Getting Started

## Installation

## Prerequisites
Make sure you have the following software installed on your machine:

- **Visual Studio:** I recommend using Visual Studio 2022 Preview.
- **.NET SDK:** Ensure you have the .NET SDK installed, targeting the `net8.0` framework.

1. Clone the repository:
git clone https://github.com/isko02/Bulky_MVC.git
cd Bulky_MVC

2. Open the Project:

- Open the Bulky_MVC.sln solution file in Visual Studio.

3. Configure the Database:

- Open appsettings.json and update the connection string in the DefaultConnection section to point to your desired database.


"ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Bulky;Trusted_Connection=True;TrustServerCertificate=True"

  },


4. Run Migrations:

Open the Package Manager Console in Visual Studio and run the following command:
Update-Database
- This will apply the database migrations and create the necessary tables.

5. Run the Application
6. Explore the Application

- Default User: 
Use the following default user to log in and explore different areas of the application:

- Admin Area:
Email: admin@iskren.com, 
Password: Qqq123*

- Customer Area:
Create a new user by clicking "Register".

