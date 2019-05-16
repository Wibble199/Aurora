
----------------------------------------------------------------------------------------
-                                     Localization                                     -
----------------------------------------------------------------------------------------

    This localization directory contains language files for Aurora.
    Adding a new language translation is easy (though it _may_ be time consuming).



                                 Adding a new language
								 ---------------------

    To add a language, first you need to find the IETF language code for the language
	and country you are translating into. For example, United States English is en-US,
	United Kingdom English is en-GB, Germany German is de-DE etc. Google should help.
	Create a folder with this tag inside Aurora's localization directory.

	Next step is to copy the "Aurora.lang" file from the en-US folder into the folder
	you just made. You can copy it from other languages if you prefer, but be warned
	that only en-US is guaranteed to be completely up-to-date with all the text.

	This file can be opened in any simple text editor (Notepad, Notepad++, VSCode, etc)
	and contains all the translated text. Each section of text has an assigned "key"
	which is how Aurora knows which text to get. The format of the lang file is:
	    <key>: <text>
	Note that the space after the colon is required. DO NOT TRANSLATE THE KEY, ONLY
	THE TEXT AFTER THE COLON. You also cannot add new lines to the translation, as
	each line indicates a new translation. If you like, you can add empty lines or
	start lines with the ~ symbol to indicate they are comments.

	Here's an example. Here are some lines from the en-US Aurora.lang file:
	    Language: Language
		PrimaryColor: Primary Color
	And here is the translated version in the de-DE Aurora.lang file:
		Language: Sprache
		PrimaryColor: Primärfarbe

	The final step required is to add an "icon.png" file into the language folder.
	The icon set used for existing icons is the color flag pack from Icons8. It can
	be found here: https://icons8.com/icon/pack/flags/color The 48px size will do.

	Once all these files are ready, you should have a directory structure something
	like this:

		Aurora/
		├── Aurora.exe
		├── Localization/
		│   ├── en-US
		│   │   ├── icon.png
		│   │   └── Aurora.lang
		│   ├── de-DE (or your language)
		│   │   ├── icon.png
		│   │   └── Aurora.lang
		│   └── Other-language-files
		└── Other-aurora-files

	After this, if you restart Aurora, it should pick up on the new language and you
	should be able to select in on the general settings tab.

	Note: If any translation keys are required and do not exist in the currently
	selected language, the en-US language will be used as a fallback, so don't worry
	about your translation breaking when a new update is released, it will simply
	show English on the text that's missing, but the rest will work.

	If you do translate, please consider making a pull request on our GitHub so that
	others can use your translation. Thank you :) You can get in touch with the
	developers on our Discord and we can help you add the translation should you
	need help or you want to ask anything.

	If there are any other .lang files inside the en-US folder, these belong to plugins
	or scripts that aren't part of Aurora normally. These can also be translated and
	if you get in touch with the author of the plugin/script, they may add your
	translation to their plugin. These files will not be added to the Aurora GitHub.

	Thank you :)
	- Wibble

----------------------------------------------------------------------------------------

	Note to developers: To add a new translation to the build, simply add the lang and
	png files to the country folder ("en-US" etc) and place inside the Localization
	folder in Visual Studio using the "Add existing item" function. Then, select the
	lang and png files and in the Properties window, set the "Copy to Output Directory"
	to be "Copy if newer". There is no need to change the build action, nor should you
	add the files to the resources.resx file.
