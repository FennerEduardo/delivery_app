Feature: Shipment Lifecycle Management
  As a logistics operator
  I want to transition shipment status through valid workflow states
  So that shipments are tracked accurately from creation to delivery

  Scenario: Transition shipment through complete successful delivery workflow
    Given a new shipment created in "Created" status
    When the shipment quote is generated
    Then the shipment status should become "Quoted"
    When the shipment is confirmed by customer
    Then the shipment status should become "Confirmed"
    When the shipment is marked as in transit
    Then the shipment status should become "InTransit"
    When the shipment is delivered to destination
    Then the shipment status should become "Delivered"
    And a status history entry should be recorded for each transition

  Scenario: Cancel shipment from Quoted status
    Given a shipment in "Quoted" status
    When the customer cancels the shipment with reason "Changed mind"
    Then the shipment status should become "Cancelled"
    And further status modifications should be blocked

  Scenario: Reject invalid status transition from Created to Delivered
    Given a shipment in "Created" status
    When an invalid status transition to "Delivered" is attempted
    Then the system should reject the request with a domain validation error
