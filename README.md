# In *Unity* scene

  - The *horses* need to be *child object* of the *Table object*.
  - *Horses* need to have the *animator component*.
  - The *maincanvas* is the canvas that user interacts with to start game.
    - Rename this objects in the scene.
       - Button to start game = "StartGame".
       - Timer text when the game starts = "Timer".
       - Speed text when the game starts = "Speed".

 # In *Visual Studio*
 To declare location for outputs
 From the tool-bar go to *Project -> Dll_Project Properties -> Build Events -> Post-build event command line*
  - copy "$(TargetDir)$(ProjectName).dll" "**YOUR PREFFERED LOCATION FOR OUTPUT** \$(ProjectName)dll.bytes"
  - copy "$(TargetDir)$(ProjectName).pdb"  "**YOUR PREFFERED LOCATION FOR OUTPUT** \$(ProjectName)pdb.bytes"
