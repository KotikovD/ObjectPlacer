## ObjectPlacer
This is a tool for Unity3D editor helps fast to place some objects while you prototyping.

![Object palcer in work](https://github.com/KotikovD/ObjectPlacer/blob/master/GIFVideo.gif "Object palcer in work")

## How to install
Copy ObjectPlacer.cs file from repository to your project in /Assets/Editor folder. (Attention: Place script necessarily in Editor folder)

## How to use
- Bake NavMesh map
- Open Object placer in Unity menu Tools/Object Placer
- Add prefabs or game objects from scene to fields, set how much each object you need
- Click to "Place" button
- For remove created objects click "Remove all" or delete "CreatedObjects" game object in Hierarchy.
- Configuring the long side of the map is necessary for even distribution of objects. The value can be set approximately.

## Recommendations
- If you place objects several times in a row, the items may intersect. It is better to immediately set the required amount for each object.
- If you want the Object placer to no longer affect the created objects, just rename the parent "CreatedObjects" in Hierarchy to any other name.
