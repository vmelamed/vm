
******************************************************************************
       Welcome to the Microsoft EventSource Samples NUGET package

**** QUICK START               ** PLEASE READ IF YOUR APP DOES NOT BUILD!

To Run the demos you need to
  * Modify your Main program to call 
	     
		 EventSourceSamples.AllSamples.Run();

  * If you did not name your application 'DemoEventSource' you must modify
    one line in the 40_LocalizedEventSource.cs and 50_LocalizedEventLogEventSource.cs
	files   Simply change 'DemoEventSource' in the LocalizationResources property
	to be the name of your application (technically it is the name of the 
	default namespace (project -> properties -> Application -> Default NameSpace)
	but this defaults to the name of the application in almost all cases. 
	
Once you do that, you can simply hit F5 and run all the demos.  

**** More Information

The samples are all under the 'EventSourceSamples' folder in your solution and
there is a file for each sample of the form NN_<SampleName>.cs. The samples
have detailed comments that tell what they do as well as WriteLine statements
that also indicate what is going on. 

So you can either simply run all the samples, or take a look at the comments
in each one to see which one is most appropriate for your needs. Each sample
has a 'Run' method that is is main entry point, so it is easy to run just
one of the samples. For example

	EventSourceSamples.EventLogEventSourceDemo.Run();

Will run just the EventLogEventSourceDemo sample. 

**** Users Guide

The Sample package also includes the _EventSourceUsersGuide.docx file
that gives you an overview of the package as a whole and how it is intended
to be used. It is worth reading. 

A useful technique is to install this package into the program that you are 
working on, cut and paste some of the sample code to your own code, and then 
uninstall the sample package (which will remove all the code you did not cut
and paste). 

By default the output goes to Console.Out but you can redirect it to another
TextWriter by setting AllSamples.Out. This is useful for GUI Apps. 

******************************************************************************
