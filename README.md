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
- git clone https://github.com/isko02/Bulky_MVC.git
- cd Bulky_MVC

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


## Usage
Bulky_MVC is designed to make bulk book purchases easy and efficient. The application is divided into two main areas:

## Admin Area
- The admin can create, edit and delete categories of books
- The admin can create, edit and delete  book companies
- The admin can manage orders.
- The admin can create, edit and delete books
- The admin can manage users.
## Customer Area
- The customer can order different books 
- The customer can manage his cart

## Employee role: 
- The employees can manage orders
## Company role: 
- If a company orders books it has 30 days to pay for them


## Project Structure:
1. Data Access (It contains the DbContext, DbInitializer which creates 1 default admin user, all migrations, Repository pattern)
2. Models (It contains all models and viewmodels)
3. Utility (It contains helper classes)
4. Test (It contains NUnit tests)
5. BulkyWeb (It contains the 2 areas, views and view components, wwwroot folder and appsettings.json)
6. BulkyWebRazor_Temp (Temporary project that I used so I can learn more about Razor pages.)


## Contributing
I welcome contributions to enhance Bulky_MVC. Follow these steps to contribute:

1. Fork the repository
2. Create a new branch
3. Make your changes
4. Open a pull request

