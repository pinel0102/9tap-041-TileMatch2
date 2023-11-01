----------------------------------------------
			  I2 SmartEdge
				  1.0.2 b3
		http://www.inter-illusion.com
		  inter.illusion@gmail.com
----------------------------------------------

Thank you for buying the I2 SmartEdge!

Documentation can be found here: http://inter-illusion.com/assets/I2SmartEdgeManual/SmartEdge.html

A few basic tutorials and more info: http://www.inter-illusion.com

If you have any questions, suggestions, comments or feature requests, please
drop by the I2 forum: http://www.inter-illusion.com/forum/index

----------------------
  Installation
----------------------

 1- Import the plugin package into a Unity project.
	If the project has NGUI or TextMeshPro installed, it needs to be Patched
		- select the menu "Tools\I2\SmartEdge\Apply NGUI Patch" or "Tools\I2\SmartEdge\Apply TextMeshPro Patch"
		- click the Patch button
	(more details: http://inter-illusion.com/assets/I2SmartEdgeManual/NGUIIntegration.html)
	
 2- Add an SmartEdge component to any Text or Label
 
 3- Change the Font to use an SDF font
	   - Either create a new one (Menu: Tools\I2\SmartEdge\SDF Font Maker), 
	   - or use a pre-built one (Assets\I2\SmartEdge\Examples\Fonts\*SDF.fontsettings)
	(more details: http://inter-illusion.com/assets/I2SmartEdgeManual/CreatingSignalDistanceFieldAsset.html)

The documentation provides further explanation on each of those steps and some tutorials.
Also its presented how to convert an existing UI into the I2 SmartEdge system.
						   (http://inter-illusion.com/assets/I2SmartEdgeManual/QuickStart.html)

-----------------------
  Troubleshooting
-----------------------

-----------------------
 Ratings
-----------------------

If you find this plugin to be useful and want to recommend others to use it. 
Please leave a review or rate it on the Asset Store. 
That will help with the sales and allow me to invest more time improving the plugin!!

Rate it at: http://u3d.as/eNV

-----------------------
 Version History
-----------------------

1.0.2 b3
NEW: Animation Sequences now have a Backward option
NEW: Lower Memory Usage: Character information is now shared between all SmartEdge components, instead of each one  having its own buffer
NEW: Animations are now not initialized until they have to be rendered. Which is a performance boost if animated objects are disabled 
NEW: Lower Memory Usage: Removed all per-character buffers for the Animations
NEW: Removed several virtual functions that were called for each vertices + for each animated range
NEW: Faster implementation of the AnimSequence: Alpha, 
NEW: Added Override/Blend mode to AnimSequence Alpha
NEW: Added 'Cull Elements' to animSequences to set intial values of elements where anim has not reached to FROM or left untouched
NEW: AnimSequences now have fading/moving values when created (e.g. alpha: 0->1, instead of 0->0 and forcing user to tweak for testing)
NEW: Exposed "Time Source" in the Animations tab. Switches between "Real" (not affected by Time.timeScale) or "Game"
NEW: Exposed "OnAnimationFinished" event in the Animation Tab
NEW: Animation Sequence "Color" now using a gradient
NEW: Added Animation Sequence to control the alpha of the Global, Face, Outline, Glow or Shadow color
NEW: Some animations sequences have a toggle to set the final value even after the animation stop (e.g. set alpha to 1, etc)
FIX: Duplicate Animation Sequence was linking the sequence values to the one that was duplicated
FIX: AnimationSequence Position will now use relative coordinates when blend mode is EXPLICIT
FIX: Inspector for Animations when set to Single (instead of loop) will stop after the animation ends
FIX: Animation coroutine will not longer generate Memory Allocs


1.0.2 a2
NEW: SmartEdge now doesn't have any Update method to improve performance (animations are now updated with a coroutine when needed)
NEW: SE RichText can now be enabled
NEW: Added a Text field in the Text Tab to show the text without any RichText or ConvertCase modifications
NEW: Better handling and detection of RichText Tags (this will be replaced by a parser, but this first step allows testing the tags)
NEW: Added a preset named 'Let It Glow'
NEW: Example Script AnimateSmartEdgeGlowUV to show how to move the UV values from a script
FIX: Arial_MSDFA had several unused textures inside, removed the ones that were not used to reduce memory

1.0.2 a1
NEW: Removed the need for an Update method in the SmartEdge. Now there is a coroutine that only do checks a few frames per second
NEW: Added a Text field to the Text Tab to show the texts without any modification (RichText tags removal, ConvertCase, etc)
NEW: RichText section (in the Text Tab) can now be enabled and will parse for tags
NEW: RichText Tags: bold [b], italic [i], alignment [right], case [uppercase]
NEW: The text tab now has a textfield that shows the original text without removing the RichText tag or modifying the case


1.0.1 b2
NEW: The most common shaders are now 30% faster
NEW: Added a new main tab ("Text") that contains all settings related to Text (Spacing, Wrapping, RichText, etc)
NEW: Added section "Text Wrapping" with controls of overflowing, wrapping, justification and paged texts
NEW: TextWrapping now also has options for Maximum Visible Characters, Words and Lines (useful for typewriting effect)
NEW: TextWrapping Vertical Overflow can now be set Page and show only the selected one
NEW: Text Modifier can now convert a text to any case (e.g. UpperCase, TitleCase, Sentence, etc) 
NEW: Spacing effect now has a parameter to control the marging of the first line in a paragraph
NEW: When creating a Kerning pair, if the "Prev Chr" is left empty, it will match any character (e.g. prev:empty, next:e, matches every 'e')
NEW: Enabling TextWrapping will disable the Alignment and overflow buttons of the text, the one in the new section will be used instead
NEW: Skipped rendering and processing when widget alpha is 0
NEW: Animation sequence can now randomize the START and DURATION of each character's action
NEW: Random values are now determistic, which lowers the memory needed for reproducing the Animation in Editor
FIX: Deform SPACING now works with Unity 5.0-5.3
FIX: When Outline layer is set to BACK, the outline color was leaking
FIX: InnerShadow was creating a black outline
FIX: Compile errors when building for Windows Store
FIX: Using Subdivisions in the Deform will not longer mess up the Outline and Glow layers
FIX: BestFit will now work correctly in NGUI and TextMeshPro when using alignments other than TopLeft
FIX: Removed error when BestFit is used without fitWidth or fitHeight
FIX: Using BestFit and Spacing made the text go outside of the bounds
FIX: Spacing will now respect the vertical and horizontal alignments
FIX: Deformations are skipped if there are no valid parameters (e.g. skip Spacing if all the spacings are 0)
FIX: Cloning an Animation fromm a preset was not copying the sequences
FIX: UI.Text bestfit can now be enabled when not using any SDF Format (otherwise, it fallbacks to the deformation BestFit)
FIX: Shaders were failing in Windows Phone
FIX: A CanvasGroups with alpha, now fades all elements correctly
FIX: When outline was enabled and its color's alpha was 0, it was showing as fully opaque
FIX: Shaders can now compile in lower OpenGL targets

1.0.0 f1
NEW: Initial Release
