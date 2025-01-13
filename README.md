<p align="center">
    <img src="https://readme-typing-svg.herokuapp.com?font=Fira+Code&weight=900&size=34&pause=1000&center=true&width=435&lines=BookStore" alt="head" />
</p>

## Project Description
The BookStore WebSite is a full-featured platform designed to provide users with an exceptional shopping experience. It includes features for browsing books, managing a cart, making secure payments, and tracking orders. The application also incorporates email notifications, multi-role access, and external authentication to enhance usability and security.
<br />

## Table of Contents

* [Deployment](#deployment)
* [Getting Started](#getting-started)
* [Requirements](#requirements)
* [ER Diagram](https://github.com/user-attachments/assets/b927bcee-5f7d-42bd-a369-c74c5a772a98)
* [Authors](#authors-black_nib)
## Deployment
Try the BookStore live: [BookStore Site](https://bulkywebstore.runasp.net/)

## Getting Started

1- Clone the repository
```bash
git clone https://github.com/gamalgithue/BookStore.git
```

2- Configure Connection String
- Open `appsettings.json` file and update the `SQL-Server` string with your SQL Server connection string.

3- Update Database
```bash
dotnet ef database update
```
4- Run the API
```bash
dotnet run
```
### Functional Requirements

* **User Roles**:

* **Admin**:

       *Manage all books, orders, and users.

       *Oversee authentication and send confirmation emails using SendGrid.

       *Monitor and manage the platform's overall functionality.

* **Customer**:

       *Browse books by category, author, or genre.

        *Add books to the cart and make payments via Stripe.

        *Receive order confirmation emails upon successful payment.

        *Track order history and view order details.

* **Employee**:

       *Assist in order processing and customer support.

### Features

       *Shopping Cart: Add, remove, and update items before checkout.

       *Payment Gateway: Secure payment integration with Stripe.

      *Order Management: Customers can view order details and track status; admins can manage orders.

* **Email Notifications**:

         *Confirmation emails to customers upon successful payment using SendGrid.

         *Email verification for user registration.

* **Authentication**:

         *Local account registration with confirmation email.

          *External login support (e.g., Facebook).

  ---
### Non-Functional Requirements

*Scalability: The system is designed to handle a growing number of users and books.

*Security: All sensitive data, including payment details, are encrypted.

*Performance: Fast response times for browsing, cart management, and checkout.

*Usability: Intuitive user interface with clear navigation for all user roles.

*Reliability: Robust error handling to ensure uninterrupted service.



