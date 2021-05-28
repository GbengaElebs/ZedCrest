# ZedCrest Project

This solution consists of 6 projects

- The Domain Layer - which is all Database Models
- The Persistence Layer - which controls access to DB
- The infrastructure Layer - which handles external infrastructure like email sending and photo uploading to external providers
- The background service - Where the RABBITMQ Consumers are implemented. This projects inherits from the Background Service
- The aApplication/Service layer- This Handles Business Logic and the implementations for  Commands and Queries flows
- The Api Layer- This handles api requests

# Documents
- I used Cloudinary to store the Documents. Create it here ---> https://cloudinary.com/users/login

# Email
- I used SendGrid and Gmails SMTP server as Email Providers

- Gmail here -->https://gmail.com/
- Send Grid here ->https://app.sendgrid.com/login/

# Database 
- MYSQL Database



