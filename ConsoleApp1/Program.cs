// See https://aka.ms/new-console-template for more information
using JsonTree;

var bucket = new SetupNodeCollection();

// add a root node to the bucket
var chassisRootContext = new SetupContext(new Guid("{1A28B39E-771B-4105-B006-4AC3051D5F92}"), "Chassis");
var chassisRootNode = new SetupNode(chassisRootContext);
bucket.AddNode(chassisRootNode);


// add a child node to the bucket and its parent node
var gearboxContext = new SetupContext(new Guid("{0F4ED5C7-0FDE-43E0-8632-8199698142F6}"), "Gearbox") { ParentContextId = chassisRootContext.Id };
var gearboxNode = new SetupNode(gearboxContext);
bucket.AddNode(gearboxNode, chassisRootNode.Id);

for (var i=1; i<9; i++)
{
    var contextId = string.Format("CF81059E-0CE9-4648-9FFB-E44EDB4E000{0}", i);
    var ratioContext = new SetupContext(new Guid(contextId), $"Ratio {i}") { ParentContextId = gearboxContext.Id };
    var ratioNode = new SetupNode(ratioContext);
    bucket.AddNode(ratioNode, gearboxNode.Id);

    var valueId = contextId.Replace("CF81059E", "75E02183");
    var ratioValueContext = new SetupContext(new Guid(valueId), $"NTeeth") { ParentContextId = ratioContext.Id };
    var ratioValueNode = new SetupNode(ratioValueContext);
    bucket.AddNode(ratioValueNode, ratioNode.Id);
}


// add a child node to the bucket (and its parent node, but we only know the parent context ID)
var driverContext = new SetupContext(new Guid("{64626B55-8544-4233-8A74-B52F1A5AF80B}"), "Driver_") { ParentContextId = chassisRootContext.Id };
var driverNode = new SetupNode(driverContext);
bucket.AddNodeToParentContext(driverNode, chassisRootContext.Id);


// multiple same children
var bumpstopsContext = new SetupContext(new Guid("{7AC55434-5BC8-4BBE-8882-7E5E9680E2A4}"), "BumpStops") { ParentContextId = chassisRootContext.Id };
var bumpstopsNode = new SetupNode(bumpstopsContext);
bucket.AddNode(bumpstopsNode, chassisRootNode.Id);
for(var i=1; i<4; i++)
{
    var contextId = string.Format("926F47AD-0EDF-41AA-B0A2-C633035D000{0}", i);
    var bsContext = new SetupContext(new Guid(contextId), "xSpring") { ParentValueOrder = i, ParentContextId = bumpstopsContext.Id };
    var bsNode = new SetupNode(bsContext);
    bucket.AddNode(bsNode, bumpstopsNode.Id);
}


var consistencyCount = bucket.EnsureConsistency();
Console.WriteLine($"Inconsistent nodes: {consistencyCount}");


// list node collection
foreach(var node in bucket)
{
    Console.WriteLine($"{node.Id} : {node.SetupContext.Name} ({node.SetupContext.Id})");
}


Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();


// find any node by node id from the bucket
var foundNode = bucket[driverNode.Id];
if (foundNode != null)
{
    Console.WriteLine($"Found driverNode {foundNode.Id} {foundNode.Name}");
}

// find any node by context id from the bucket
var foundContext = bucket.FindByContextId(driverContext.Id);
if (foundContext != null)
{
    Console.WriteLine($"Found driverNode by context id {foundContext.Id} {foundContext.Name}");
}


// retrieve a node by JSON path from bucket
var foundGbx = bucket["Chassis.Gearbox"];
if (foundGbx != null)
{
    Console.WriteLine($"Found gbxNode by path from bucket : {foundGbx.Id} {foundGbx.Name}");
}


// retrieve a node by JSON path from parent node
var foundDrv = chassisRootNode["Driver_"];
if (foundDrv != null)
{
    Console.WriteLine($"Found driverNode by path from parent : {foundDrv.Id} {foundDrv.Name}");
}


// retrieve a node by JSON path with array from bucket
var foundValue = bucket["Chassis.BumpStops[2].xSpring"];
// "Chassis.BumpStops.2.xSpring"
if (foundValue != null)
{
    Console.WriteLine($"Found array node by path from bucket : {foundValue.Id} {foundValue.Name}");
}


Console.ReadLine();