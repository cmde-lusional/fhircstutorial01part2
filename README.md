# FHIR-CS-Tutorial-01

This is a basic .Net Core CLI project to interact with an open FHIR Server.

# Documentation

## Create a Resource

For creating a resource it as necessary to look at the documentation of a resource for checking wether a resources contents have a cardinality of 1..1 or not. In case any resource content does have cardinility of 1..1 then it needs to be declared in the function of the app which is creating the resource.

In example could be to create a new patient. The resource contents can be found here:

[Patient](https://www.hl7.org/fhir/patient.html)

The resource contents necessary for a patient are:
- communication/language
- link/other
- link/type

This means in case we use the communication or the link content we also need to declare language or other and type or all of them.

Creating a resource is using the setter syntax von C#.

KEEP IN MIND
The creation process in Fhir caches created resources which means after running the app for creating a user the second time we don't necessarily see that a second user was created. The reason here is that the user has been created, but not indexed yet.
If you want to see the created patient on the output you can use the following lines at the end of the create function:

`Patient patientCreated = fhirClient.Create<Patient>(patientToCreate);
Console.WriteLine($"Patient created: {patientCreated.Id}");`

## More Information

FHIR&reg; is the registered trademark of HL7 and is used with the permission of HL7.
