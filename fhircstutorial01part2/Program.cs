// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;


// connection var for fire server
const string _fhirServer = "http://server.fire.ly";

// read in settings for following fire server connection
var settings = new FhirClientSettings
{
  PreferredFormat = ResourceFormat.Json,
  PreferredReturn = Prefer.ReturnRepresentation
};

// creates a new fire client connecting to a fire server defined in var _fhirServer
var fhirClient = new FhirClient(_fhirServer, settings);

Bundle patientBundle = fhirClient.Search<Patient>(new string[] {"name=Peter"});

int patientNumber = 0;
List<string> patientsWithEncounters = new List<string>();

while (patientBundle != null)
{
  Console.WriteLine($"Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");

  // list each patient in the bundle
  foreach (Bundle.EntryComponent entry in patientBundle.Entry)
  {
    if (entry.Resource != null)
    {
      Patient patient = (Patient)entry.Resource;

      Bundle encounterBundle = fhirClient.Search<Encounter>(
        new string[]
        {
            $"patient=Patient/{patient.Id}",
        });

      if (encounterBundle.Total == 0)
      {
        continue;
      }

      patientsWithEncounters.Add(patient.Id);

      Console.WriteLine($"- Entry {patientNumber,3}: {entry.FullUrl}");
      Console.WriteLine($" -   Id: {patient.Id}");

      if (patient.Name.Count > 0)
      {
        Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
      }

      Console.WriteLine($" - Encounters Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");
    }

    patientNumber++;

    if (patientsWithEncounters.Count >= 2)
    {
      break;
    }
  }

  if (patientsWithEncounters.Count >= 2)
  {
    break;
  }

  // get more results
  patientBundle = fhirClient.Continue(patientBundle);
}