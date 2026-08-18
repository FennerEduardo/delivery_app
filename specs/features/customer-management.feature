Feature: Customer Management
  As a logistics platform user
  I want to register customer accounts with contact details
  So that shipments can be assigned to valid customers

  Scenario: Successfully register a new customer
    Given customer registration details:
      | Name       | Email                  | Phone        | Street          | City     | Country  |
      | Acme Corp  | contact@acmecorp.com   | +573001234567 | Av El Dorado 68 | Bogota   | Colombia |
    When the customer registration is submitted
    Then a new customer ID should be generated
    And the customer status should be active

  Scenario: Fail registration with invalid email format
    Given customer registration details with invalid email "invalid-email"
    When the customer registration is submitted
    Then the system should reject the request with validation error "Invalid email format"
