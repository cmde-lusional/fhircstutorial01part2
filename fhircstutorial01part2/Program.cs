// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

// connection dic for fire server
Dictionary<string, string> _fhirServers = new Dictionary<string, string>()
{
  { "PublicVonk", "http://server.fire.ly" },
  { "PublicHAPITW", "https://hapi.fhir.tw/" },
  { "Local", "http://localhost:8080/fhir" },
};

// connection var for fire server
string _fhirServer = _fhirServers["Local"];


// read in settings for following fire server connection
var settings = new FhirClientSettings
{
  PreferredFormat = ResourceFormat.Json,
  PreferredReturn = Prefer.ReturnRepresentation
};

// creates a new fire client connecting to a fire server defined in var _fhirServer
var fhirClient = new FhirClient(_fhirServer, settings);

//CreatePatient(fhirClient, "Tom", "Hardy");

List<Patient> patients = GetPatients(fhirClient);

Console.WriteLine($"Found {patients.Count} patients");

string firstId = null;

//Delete all patients accept first one
foreach (Patient patient in patients)
{
  if (firstId == null)
  {
    firstId = patient.Id;
    continue;
  }
  deletePatient(fhirClient, patient.Id);
}

Patient firstPatient = readPatient(fhirClient, firstId);
Console.WriteLine($"Read back Patient: {firstPatient.Name[0].ToString()}");

Patient updated = updatePatient(fhirClient, firstPatient);

readPatient(fhirClient, firstId);

//Read a patient from a fire server by ID
static Patient readPatient(FhirClient fhirClient, string id)
{
  if (string.IsNullOrEmpty(id))
  {
    throw new ArgumentNullException(nameof(id));
  }

  Console.WriteLine($"Reading Patient {id}");
  Patient readPatient = fhirClient.Read<Patient>($"Patient/{id}");
  return readPatient;
}

//Update a patient
static Patient updatePatient(FhirClient fhirClient, Patient patient)
{
  patient.Telecom.Add(new ContactPoint(){ 
    System = ContactPoint.ContactPointSystem.Phone,
    Value = "555.555.5555",
    Use = ContactPoint.ContactPointUse.Home
  });

  patient.Gender = AdministrativeGender.Unknown;

  Patient updatedPatient = fhirClient.Update<Patient>(patient);

  return updatedPatient;

}

static void CreatePatient(FhirClient fhirClient, string familyName, string givenName)
{
  Patient patientToCreate = new Patient()
  {
    Name = new List<HumanName>()
    {
      new HumanName()
      {
        Family = familyName,
        Given = new List<string>()
        {
          givenName,
        },
      }
    },
    BirthDateElement = new Date(2002, 07,07)
  };

  Patient patientCreated = fhirClient.Create<Patient>(patientToCreate);
  Console.WriteLine($"ID of created Patient {patientCreated.Id}");
  
}

//Delete a patient specified by ID
static void deletePatient(FhirClient fhirClient, string id)
{
  if (string.IsNullOrEmpty(id))
  {
    throw new ArgumentNullException(nameof(id));
  }

  Console.WriteLine($"Deleting patient: {id}");
  fhirClient.Delete($"Patient/{id}");
  
}

//Function to retrieve patients
static List<Patient> GetPatients(FhirClient fhirClient, string[] patientCriteria = null, int maxPatients = 20, bool onlyWithEncounters = false)
{
  List<Patient> patients = new List<Patient>();
  Bundle patientBundle = new Bundle();
  if (patientCriteria == null || patientCriteria.Length == 0)
  {
    patientBundle = fhirClient.Search<Patient>();
  }
  else
  {
    patientBundle = fhirClient.Search<Patient>(patientCriteria);
  }

  while (patientBundle != null)
  {
    Console.WriteLine($"Patient Bundle.Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");
  
    // list each patient in the bundle
    foreach (Bundle.EntryComponent entry in patientBundle.Entry)
    {
      if (entry.Resource != null)
      {
        Patient patient = (Patient)entry.Resource;
  
        Bundle encounterBundle = fhirClient.Search<Encounter>(
          new string[]
          {
              $"subject=Patient/{patient.Id}",
          });
  
        if (onlyWithEncounters && encounterBundle.Total == 0)
        {
          continue;
        }
  
        patients.Add(patient);
  
        Console.WriteLine($"- Entry {patients.Count,3}: {entry.FullUrl}");
        Console.WriteLine($" -   Id: {patient.Id}");
  
        if (patient.Name.Count > 0)
        {
          Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
        }

        if (encounterBundle.Total > 0)
        {
          Console.WriteLine($" - Encounters Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");
        }
      }
      
      if (patients.Count >= maxPatients)
      {
        break;
      }
      
    }
  
    if (patients.Count >= maxPatients)
    {
      break;
    }
  
    // get more results
    patientBundle = fhirClient.Continue(patientBundle);
  }
  
  return patients;
}



