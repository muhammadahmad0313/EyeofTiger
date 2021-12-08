//****************************************************
//Unity Package: Boxing Arena
//Developer: Oddity Interactive
//email: support@oddityinteractive.com
//web: http://www.oddityinteractive.com
//****************************************************
//
//
//----------------------------------------------------
//description of unity package
//----------------------------------------------------
// A package containing a boxing ring with rope dynamics and 
// dynamic rope shadows, as well as a camera model for a player vs ai or player vs player.
// the two players look at eachother and the distance between them creates an offset
// for the camera to keep both in frame however far apart they are (fully customisable script).
// Includes some decent meshes so you can hit the ground running
// on a fighting game or however you want to use them with Unity.
//
//
//****************************************************
//usful information
//****************************************************
//----------------------------------------------------
//-Getting started.
//----------------------------------------------------
// Start a new unity project and import the package.
// The package contains all necessary elements in order to use.
//
// There is also a demo scene that allows you to control one of the players.
//
//----------------------------------------------------
//Controls - Demo Scene
//----------------------------------------------------
//Player 1 - w, s, a, d - (movement)
//Player 2 - up, down, left, right (movement)
//----------------------------------------------------
//-Prefab.
//----------------------------------------------------
// the prefab "Whole Scene" contains all the elements ready to drop into
// the hierachy. you can then if you wish seperate them from the prefab.
//
//----------------------------------------------------
//-"Layers".
//----------------------------------------------------
// The project contains 3 additional layers,
// "Rope Static", "Rope Dynamic" and "Player Mesh"
// The Physics for these two layers do not interact.
// Note: if adding the prefabs and meshes alone to a project
//       you will have to create these two layer and assign them to
// "Rope Static" - Whole Scene\Boxing Ring - Static\Static Body - Ropes
// "Rope Dynamic" - Whole Scene\Boxing Ring - Rope Dynamics (and all decendants).
// "Player Mesh" - this layer is for the player feet fake shadow projections so the projectors 
//				   don't display on the player meshes. 
//
//----------------------------------------------------
//-"Arena Lighting".
//----------------------------------------------------
// There are no shadows currently in the project as fake soft shadows are being used.
// There are 8 spot lights lighting up the scene.
//
// Stage Light G5 A,Stage Light G5 B,Stage Light G5 C and Stage Light G5 D
// are sets of 5 mesh visual representing stage lights, they can be pitch and also effect the
// spot light that is attached to it (though keep in mind you would need to hide the fake shadows
// if moving the lighting or create new shadow textures for the new lighting values)
//
// There is also lens flares (20 of them) that are controled by scripts to look at the camera. 
// so remember if you add that to a seperate project you will have to assign the camera to the lens flare
// scripts attached to them.
//
// Each lens flare A - D, 1 - 5 have been placed as children of the bones for the "Stage Light G5 x"
//
//----------------------------------------------------
//-"Boxing Ring - Camera".
//----------------------------------------------------
// The camera model being used is one that keeps a dynamic offset relative to the two players.
// This is controlled by a documented script.
//
//----------------------------------------------------
//-"Boxing Ring - Rope Dynamics".
//----------------------------------------------------
// The Mesh "boxing_ring_ropes" contains bones that have been placed
// as childrem of physx rigid bodies that make up the ring. these are linked
// by spring joints.
//
// because character controllers don't interact with rigid bodies by default
// a script has been added to the player 1 cylinder (character controller)
// so that in can interact with the ropes.
//
// the ring floor (not outer floor) fake shadows also have bones and move the shadows in the same fashion.
//
//----------------------------------------------------
//- Collision Notes
//----------------------------------------------------
// There are 3 main collision bodies not counting the players.
// "Static Body - Ground" - "default" Layer
// "Static Body - Ropes" - "rope static" Layer
// and all the rigid bodies that make up the dynamic rope - "Rope Dynamic" Layer.
// 
// "Rope Static" and "Rope Dynamic" don't interact with eachother but both
// can interact with the default layer. This provides a solid barier for the players
// so they don't fall out of the ring by pushing on the ropes to hard, yet also
// dynamic interaction to simulate the physics of the ropes of a boxing ring.
//
//----------------------------------------------------
//- Script execution order
//----------------------------------------------------
//for smoothest movement of the players charactor controller and the camera model being used.
//the script execution order should be:
//
// Player_1_InputController
// Player_2_InputController
// BoxingCameraManager
// PlayerProjectorFakeShadow
//
//the rest of the scripts in the project are fine to execute as normal.














 


