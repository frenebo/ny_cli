﻿using CommandLine;
using FSOpsNS;
using System.Collections.Generic;
using PackageManagerNS;
using CommandLine.Text;
using Constants;
using CLIInterfaceNS;
using System.Linq;
using FileTypeInferenceNS;

namespace nyoka_cli
{
    [Verb("init", HelpText = "Initialize code, data and model folders.")]
    class InitOptions 
    {
    }

    [Verb("add", HelpText = "Add resource")]
    class AddOptions
    {
        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Value(2, Required = false, HelpText = "Resource version")]
        public string version {get;set;}
        
        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "Add a data resource",
                        new AddOptions
                        {
                            resourceName = "example_data_resource_name.json"
                        }
                    )
                };
            }
        }
    }

    [Verb("remove", HelpText = "Remove resource")]
    class RemoveOptions
    {
        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}


        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "Add a data resource",
                        new RemoveOptions
                        {
                            resourceName = "example_model_resource_name.csv"
                        }
                    )
                };
            }
        }
    }

    [Verb("list", HelpText = "List packages")]
    class ListOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        [Usage(ApplicationAlias = ConstStrings.APPLICATION_ALIAS)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "List all resources",
                        new ListOptions {}
                    ),
                    new Example(
                        "List all model resources",
                        new ListOptions
                        {
                            resourceType = "model"
                        }
                    )
                };
            }
        }
    }

    [Verb("available", HelpText = "List available packages")]
    class AvailableOptions
    {
        [Value(0, Required = false, HelpText = ConstStrings.RESOURCE_TYPE_HINT)]
        public string resourceType {get;set;}

        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "List all available resources",
                        new AvailableOptions {}
                    ),
                    new Example(
                        "List all available model resources",
                        new AvailableOptions {
                            resourceType = "model"
                        }
                    )
                };
            }
        }
    }

    [Verb("dependencies", HelpText = "List dependencies of resource")]
    class DependenciesOptions
    {
        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Value(2, Required = false, HelpText = "Resource version")]
        public string version {get;set;} = null;

        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(
                        "List the dependencies of a locally downloaded code resource",
                        new DependenciesOptions {
                            resourceName = "some_local_resource_name.py"
                        }
                    ),
                    new Example(
                        "List the dependencies of a model resource",
                        new DependenciesOptions {
                            resourceName = "name_of_server_model.pmml",
                            version = "1.2.3",
                        }
                    )
                };
            }
        }
    }

    // [Verb("pruneto", HelpText = "Prune the given resource type to the provided resources")]
    // class PruneToOptions
    // {
    //     [Option("code", Separator=',', HelpText = "code resource(s) to keep, separated by commas")]
    //     public IEnumerable<string> keepCode {get;set;}

    //     [Option("model", Separator=',', HelpText = "model resource(s) to keep, separated by commas")]
    //     public IEnumerable<string> keepModel {get;set;}

    //     [Option("data", Separator=',', HelpText = "data resource(s) to keep, separated by commas")]
    //     public IEnumerable<string> keepData {get;set;}

    //     public IEnumerable<Example> Examples
    //     {
    //         get
    //         {
    //             return new List<Example>()
    //             {
    //                 new Example("Keep a code resource and two model resources", new PruneToOptions {
    //                     keepCode= new string[] {"some_code.py"},
    //                     keepModel= new string[] {"model1.pmml", "model2.pmml"},
    //                     keepData= new string[] {},
    //                 }),
    //             };
    //         }
    //     }
    // }

    [Verb("publish", HelpText = "Publish a resource to server")]
    class PublishOptions
    {
        [Value(1, Required = true, HelpText = "Resource name")]
        public string resourceName {get;set;}

        [Value(2, Required = true, HelpText = "Resource version")]
        public string version {get;set;}

        [Option("codedeps", Separator=',', HelpText = "code depedencies, separated by commas. Example: code.py@1.2.3")]
        public IEnumerable<string> codeDeps {get;set;}

        [Option("modeldeps", Separator=',', HelpText = "model depedencies, separated by commas. Example: model39.pmml@13.4.2")]
        public IEnumerable<string> modelDeps {get;set;}

        [Option("datadeps", Separator=',', HelpText = "data depedencies, separated by commas. Example: dataset.csv@1.0.0")]
        public IEnumerable<string> dataDeps {get;set;}

        public IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Publish code_file.ipynb version 1.2.3 with no dependencies", new PublishOptions {
                        resourceName = "code_file.ipynb",
                        version = "1.2.3",
                    }),
                    new Example("Publish model1.pmml version 10.2.3 with a data dependency called dataset.json, version 1.2.3", new PublishOptions {
                        resourceName = "model1.pmml",
                        version = "10.2.3",
                        dataDeps = new string[] {"dataset.json@1.2.3"},
                    }),
                };
            }
        }
    }

    class Program
    {
        internal class ArgumentProcessException : System.Exception
        {
            public ArgumentProcessException(string mssg)
            : base (mssg)
            {
            }
        }

        private static ResourceType parseResourceType(string type)
        {
            if (type.ToLower() == "model") return ResourceType.model;
            else if (type.ToLower() == "data") return ResourceType.data;
            else if (type.ToLower() == "code") return ResourceType.code;
            else throw new ArgumentProcessException($"Invalid resource type \"{type}\"");
        }
        
        private static ResourceType inferResourceTypeFromResourceName(string resourceName)
        {
            try{
                if (FileTypeInference.isCodeFileName(resourceName)) return ResourceType.code;
                if (FileTypeInference.isDataFileName(resourceName)) return ResourceType.data;
                if (FileTypeInference.isModelFileName(resourceName)) return ResourceType.model;
                throw new System.Exception();
            }
            catch (System.Exception)
            {
                throw new ArgumentProcessException($"Could not infer resource type from extension of {resourceName}");
            }
        }
        
        static void Main(string[] args)
        {
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = System.Console.Error;
            });

            parser.ParseArguments<InitOptions, AddOptions, RemoveOptions, ListOptions, AvailableOptions, DependenciesOptions, PublishOptions>(args)
                .WithParsed<InitOptions>(opts => {
                    PackageManager.initDirectories();
                })
                .WithParsed<ListOptions>(opts => {
                    if (opts.resourceType == null)
                    {
                        PackageManager.listResources(null);
                    }
                    else
                    {
                        try
                        {
                            ResourceType resourceType = parseResourceType(opts.resourceType);
                            PackageManager.listResources(resourceType);
                        }
                        catch (ArgumentProcessException ex)
                        {
                            CLIInterface.logError(ex.Message);
                        }
                    }
                })
                .WithParsed<AddOptions>(opts => {
                    try
                    {
                        PackageManager.addPackage(
                            inferResourceTypeFromResourceName(opts.resourceName),
                            opts.resourceName,
                            opts.version // possible null
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                .WithParsed<RemoveOptions>(opts => {
                    try
                    {
                        PackageManager.removePackage(
                            inferResourceTypeFromResourceName(opts.resourceName),
                            opts.resourceName
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                .WithParsed<AvailableOptions>(opts => {
                    if (opts.resourceType == null)
                    {
                        PackageManager.listAvailableResources(null);
                    }
                    else
                    {
                        try
                        {
                            ResourceType resourceType = parseResourceType(opts.resourceType);
                            PackageManager.listAvailableResources(resourceType);
                        }
                        catch (ArgumentProcessException ex)
                        {
                            CLIInterface.logError(ex.Message);
                        }
                    }
                })
                .WithParsed<DependenciesOptions>(opts => {
                    try
                    {
                        PackageManager.listDependencies(
                            inferResourceTypeFromResourceName(opts.resourceName),
                            opts.resourceName,
                            opts.version
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                })
                // .WithParsed<PruneToOptions>(opts => {
                //     PackageManager.pruneTo(
                //       opts.keepCode.ToList(),
                //       opts.keepData.ToList(),
                //       opts.keepModel.ToList()
                //     );
                // })
                .WithParsed<PublishOptions>(opts => {
                    try
                    {
                        PackageManager.publishResource(
                            inferResourceTypeFromResourceName(opts.resourceName),
                            opts.resourceName,
                            opts.version,
                            opts.codeDeps.ToList(),
                            opts.dataDeps.ToList(),
                            opts.modelDeps.ToList()
                        );
                    }
                    catch (ArgumentProcessException ex)
                    {
                        CLIInterface.logError(ex.Message);
                    }
                });
        }
    }
}
