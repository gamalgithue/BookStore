<p align="center">
    <img src="https://readme-typing-svg.herokuapp.com?font=Fira+Code&weight=900&size=34&pause=1000&center=true&width=435&lines=BookStore" alt="head" />
</p>

## Project Description
The BookStore WebSite is a full-featured platform designed to provide users with an exceptional shopping experience. It includes features for browsing books, managing a cart, making secure payments, and tracking orders. The application also incorporates email notifications, multi-role access, and external authentication to enhance usability and security.
<br />

## Table of Contents

* [Deployment](#deployment)
* [Requirements](#requirements)
* [Getting Started](#getting-started)
* [ER Diagram](https://github.com/gamalgithue/BookStore/issues/1#issue-2784771728)
* [Technology Tools](#technology-tools)

## Deployment
Try the BookStore WebSite live: [BookStore](https://bulkywebstore.runasp.net/)

----------

 ## Requirements
 ### Functional Requirements

#### User Roles

1. **Admin**:

   - Manage all books, orders, and users.
   - Oversee authentication and send confirmation emails using SendGrid.
   - Monitor and manage the platform's overall functionality.

2. **Customer**:

   - Browse books by category, author, or genre.
   - Add books to the cart and make payments via Stripe.
   - Receive order confirmation emails upon successful payment.
   - Track order history and view order details.

3. **Company**:

   - Manage inventory and update book details (pricing, stock, etc.).
   - Coordinate with admin for large-scale operations.

4. **Employee**:

   - Assist in order processing and customer support.

### Features

- **Shopping Cart**: Add, remove, and update items before checkout.
- **Payment Gateway**: Secure payment integration with Stripe.
- **Order Management**: Customers can view order details and track status; admins can manage orders.
- **Email Notifications**:
  - Confirmation emails to customers upon successful payment using SendGrid.
  - Email verification for user registration.
- **Authentication**:
  - Local account registration with confirmation email.
  - External login support (e.g., Facebook)
    
-----

### Non-Functional Requirements

- **Scalability**: The system is designed to handle a growing number of users and books.

- **Security**: All sensitive data, including payment details, are encrypted.

- **Performance**: Fast response times for browsing, cart management, and checkout.

- **Usability**: Intuitive user interface with clear navigation for all user roles.

- **Reliability**: Robust error handling to ensure uninterrupted service.

------

## Getting Started

### Prerequisites

- Visual Studio 2022
- SQL Server 2022 
- ASP.NET Core 8.0
- Stripe API key and SendGrid API key

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/gamalgithue/BookStore.git
   ```
2. Open the project in Visual Studio.
3. Restore NuGet packages by building the solution.
4. Configure the following in `appsettings.json`:
   - Database connection string
   - Stripe API key
   - SendGrid API key
5. Run database migrations or execute the provided SQL scripts to set up the database.
6. Start the application using IIS Express or a similar server.


## Technology Tools

- **Frontend**: HTML, CSS, JavaScript, and jQuery.
- **Backend**: ASP.NET Core MVC for handling server-side logic.
- **Database**: SQL Server for storing users, books, orders, and other data.
- **Payment Gateway**: Stripe for secure and reliable transactions.
- **Email Service**: SendGrid for sending confirmation and notification emails.



