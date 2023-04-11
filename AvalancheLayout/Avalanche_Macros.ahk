F20::Send {Home}{Shift down}{End}{Shift up}

^F20::Send {Home}{Shift down}{End}{Shift up}^{c}

+F20::Send {Home}{Shift down}{End}{Shift up}^{v}

F14::
;EN
      SetDefaultKeyboard(0x0409)
      ;DllCall("ActivateKeyboardLayout", "UInt", 0x00000409, "UInt", 0x00000001)
return

F15::
;RU
      SetDefaultKeyboard(0x0419)
return

F16::
      SetTitleMatchMode, 2
      WinActivate, Notepad ;Will match any window name containing 'Notepad'
Return


SetDefaultKeyboard(LocaleID){
	Global
	SPI_SETDEFAULTINPUTLANG := 0x005A
	SPIF_SENDWININICHANGE := 2
	Lan := DllCall("LoadKeyboardLayout", "Str", Format("{:08x}", LocaleID), "Int", 0)
	VarSetCapacity(Lan%LocaleID%, 4, 0)
	NumPut(LocaleID, Lan%LocaleID%)
	DllCall("SystemParametersInfo", "UInt", SPI_SETDEFAULTINPUTLANG, "UInt", 0, "UPtr", &Lan%LocaleID%, "UInt", SPIF_SENDWININICHANGE)
	WinGet, windows, List
	Loop %windows% {
		PostMessage 0x50, 0, %Lan%, , % "ahk_id " windows%A_Index%
	}
}