Feature: Shipping Quote Calculation
  As a logistics operator or customer
  I want to calculate shipment costs with detailed cost breakdowns
  So that I understand exact pricing factors, surcharges, and total rates

  Scenario: Calculate standard shipping quote for lightweight item
    Given a customer with ID "cust-001"
    And a shipment with actual weight 3.0 kg
    And package dimensions length 20 cm, width 15 cm, height 10 cm
    And a commercial value of 200000 COP
    And a delivery distance of 25 km
    And a delivery type "Standard"
    And a delivery time window "Standard"
    When the shipping quote is calculated
    Then the billable weight should be 3.0 kg
    And the base cost should be 15000 COP
    And the distance surcharge should be 1500 COP
    And the commercial value surcharge should be 0 COP
    And the total shipping cost should be 16500 COP

  Scenario: Calculate quote where volumetric weight exceeds actual weight
    Given a shipment with actual weight 2.0 kg
    And package dimensions length 50 cm, width 40 cm, height 30 cm
    And a commercial value of 100000 COP
    And a delivery distance of 5 km
    And a delivery type "Standard"
    And a delivery time window "Standard"
    When the shipping quote is calculated
    Then the volumetric weight should be 16.0 kg
    And the billable weight should be 16.0 kg
    And the base cost should be 35000 COP
    And the total shipping cost should be 35000 COP

  Scenario: Calculate express delivery with weekend surcharge
    Given a shipment with actual weight 5.0 kg
    And package dimensions length 10 cm, width 10 cm, height 10 cm
    And a commercial value of 3000000 COP
    And a delivery distance of 40 km
    And a delivery type "Express"
    And a delivery time window "Weekend"
    When the shipping quote is calculated
    Then the system should provide an itemized cost breakdown with base, distance, value, express, and weekend surcharges
