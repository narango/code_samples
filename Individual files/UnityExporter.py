__author__ = "Narendra 'aka' NaRango"
'''
brief: Exports the base and low-version assets separately to a valid Unity Project. It exports in FBX export. For use in Autodesk Maya.

Exports assets directly to selected Unity Project
valid Assets = models
 
USAGE:
1) Execute this script in Maya's Script Editor
2) It should open "Unity Exporter" window
'''

import maya.cmds as cmds
import functools
import logging
import maya.mel as mel

log = logging.getLogger("UnityExporterLog")

# global variables that can be used by other tools
WINDOW_UI = "unityExporter"
TITLE = "Unity Exporter"
SIZE = (545, 350)
ACTION_NAME = "Export"
IS_SIZABLE = False
HAS_MAXBUTTON = False

_mainForm = None
_applyBtn = None

_ToolUI = {}
_uiValues = {}

_path = ""
_assets = {"normal":"","low" : ""}

#Common UI can be used for any other tool
def UI():
    """
        Creates a base window
    """
    global WINDOW_UI, TITLE, SIZE, ACTION_NAME, HAS_EDIT, IS_SIZABLE, HAS_MAXBUTTON, _applyBtn, _mainForm
    # check for window existence
    if cmds.window(WINDOW_UI, exists=True):
        cmds.deleteUI(WINDOW_UI, window=True)

    # create window
    WINDOW_UI = cmds.window(WINDOW_UI, title=TITLE, wh=SIZE, menuBar=True, mxb=HAS_MAXBUTTON, sizeable=IS_SIZABLE)
    # create Form
    _mainForm = cmds.formLayout("mainForm", nd=100)
    # create common menu
    #_ComonMenu()
    # create command buttons
    _CommandButtons()
    # create a tab layout to place tool's options
    optionsTab = cmds.tabLayout(scrollable=True, tabsVisible=False, height=1)
    cmds.formLayout(_mainForm, e=True,
                    attachForm=[(optionsTab, "top", 0),
                                (optionsTab, "left", 2),
                                (optionsTab, "right", 0)],
                    attachControl=[(optionsTab, "bottom", 5, _applyBtn)])
    # create a separate from to layout Tool options
    createFrom = cmds.formLayout("createFrom", nd=100)
    ToolOptions()

    # show window
    cmds.showWindow(WINDOW_UI)
    cmds.window(WINDOW_UI, edit=True, wh=SIZE)
    pass

#Common Menu can be used for any other tool
#def _ComonMenu():
#    """
#        a common menu that will be displayed in al nyTool windows
#    """
#    global TITLE
#    # create Edit Menu
#    editMenu = cmds.menu(l="Edit")
#    editMenuSave = cmds.menuItem(l="Save Settings")
#    editMenuReset = cmds.menuItem(l="Reset Settings")
#    cmds.menuItem(d=True)
#    helpMenu = cmds.menu(l="Help")
#    helpMenuHelp = cmds.menuItem(l="Help on %s" % TITLE)
#    pass

#Command Buttons
def _CommandButtons():
    """
        command buttons to perform actions of specific Tool
    """
    global _applyBtn, _mainForm
    # create buttons
    commandButtonSize = (SIZE[0] - 18 / 3, 26)
    actionBtn = cmds.button(l="%s" % ACTION_NAME, h=commandButtonSize[1], c=_action_tool, ann="Apply the settings and close the tool window")
    _applyBtn = cmds.button(l="Apply", h=commandButtonSize[1], c=ApplyTool, ann="Apply the settings")
    closeBtn = cmds.button(l="Close", h=commandButtonSize[1], c=CloseTool, ann="Close the tool window")
    #Layout Buttons
    cmds.formLayout(_mainForm, e=True,
                    attachForm=[(actionBtn, "left", 5),
                                (actionBtn, "bottom", 5),
                                (_applyBtn, "bottom", 5),
                                (closeBtn, "bottom", 5),
                                (closeBtn, "right", 5)],
                    attachPosition=[(actionBtn, "right", 1, 33),
                                    (closeBtn, "left", 0, 67)],
                    attachControl=[(_applyBtn, "left", 5, actionBtn),
                                   (_applyBtn, "right", 5, closeBtn)],
                    attachNone=[(actionBtn, "top"),
                                (_applyBtn, "top"),
                                (closeBtn, "top")])

    pass


def ToolOptions():
    """
        UI Options for the tool, which will be displayed along with common UI
    """
    global _ToolUI
    cmds.columnLayout()
    cmds.rowColumnLayout(nc=2, cw=[(1, 175)], co=[(1, "right", 5)])
    #field - get location
    cmds.text(l="Unity Project Location:")
    cmds.rowLayout(nc=2)
    _ToolUI["txt_unityLocation"] = cmds.textField(w = SIZE[0]/2, editable=False, text="Select a valid Unity Project Directory", ann="this field is not writable")
    _ToolUI["btn_unityLocation"] = cmds.button(l="  ..  ", c=_GetUnityPath, ann = "Select a valid Unity Project Directory")
    cmds.setParent('..')
    #Directory Name
    cmds.text(l="Unity Folder Name:")
    _ToolUI["txt_folderName"] = cmds.textField(w=SIZE[0]/2, text="", ann="Give an existing folder/Create a new one")
    #Asset Name
    cmds.text(l="Asset Name:")
    _ToolUI["txt_assetName"] = cmds.textField(w=SIZE[0]/2, text="", ann="Give an existing file/Create a new one")
    #field - select export format
    cmds.text(l="Format:")
    _ToolUI["radio_format"] = cmds.radioButtonGrp(label1="FBX", numberOfRadioButtons=1, select=1,
                                                     cw1=50, en=False)

    #select Normal mesh
    cmds.text(l="Normal Assets:")
    cmds.rowLayout(nc=2)
    _ToolUI["txt_normalAssets"] = cmds.textField(w = SIZE[0]/2, editable=False, text="Select Assets which are in Normal version", ann="this field is not writable")
    _ToolUI["btn_normalAssets"] = cmds.button(l="  <<  ", c=functools.partial(_GetAssets,False), ann="Link your selection here")
    cmds.setParent('..')
    #select Low mesh
    cmds.text(l="Low Version Assets:")
    cmds.rowLayout(nc=2)
    _ToolUI["txt_lowAssets"] = cmds.textField(w = SIZE[0]/2, editable=False, text="Select Assets which are in Low version", ann="this field is not writable")
    _ToolUI["btn_lowAssets"] = cmds.button(l="  <<  ", c=functools.partial(_GetAssets,True), ann="Link your selection here")


# Get a Valid Unity Project Directory
def _GetUnityPath(*args):
    global _ToolUI, _path
    #Show the Dialog window with Directory selection
    _path = cmds.fileDialog2(fm=3, okc = "Select", cap="select Unity Project")
    #Check if the Directory contains "Assets" and "Project Settings" as sub-folders
    if _path:
        _path = _path[0]
        file1Exists =  cmds.file("%s/Assets" % _path, q=True, exists=True)
        file2Exists =  cmds.file("%s/ProjectSettings" % _path, q=True, exists=True)
        #display the path to the user if the directory is valid
        if(file1Exists and file2Exists):
            cmds.textField(_ToolUI["txt_unityLocation"], e=True, text=_path, ebg=True, bgc=[0, .2, 0])
            _path=_path + "/Assets/"+ cmds.textField(_ToolUI["txt_folderName"], q=True, text=True) 
        else:
            #display warning to if the directory is not valid
            _path = ""
            cmds.textField(_ToolUI["txt_unityLocation"], e=True, text="Invalid Directory. Select a valid Unity Project Directory", ebg=True, bgc=[.2, 0, 0])
        log.info("Assets: %s | ProjectSettings: %s" % (file1Exists, file2Exists))
    else:
        #display warning to if the directory is not valid
        _path = ""
        cmds.textField(_ToolUI["txt_unityLocation"], e=True, text="Not valid. Select a valid Unity Project Directory", ebg=True, bgc=[.2, 0, 0])
    log.info(_path)
    return _path

# Get a Valid Directory. A Directory that exists on disk
def _GetValidPath(*args):
    global _ToolUI, _path
    #Show the Dialog window with Directory selection
    _path = cmds.fileDialog2(fm=3, okc = "Select", cap="select Unity Project")
    if _path:
        _path = _path[0]
    else:
        _path = ""
    #display the path
    if(_path):
        cmds.textField(_ToolUI["txt_unityLocation"], e=True, text=_path, ebg=True, bgc=[0, .2, 0])
    else:
        cmds.textField(_ToolUI["txt_unityLocation"], e=True, text="Not valid. Select a valid Directory", ebg=True, bgc=[.2, 0, 0])
    log.info(_path)
    return _path

#Get assets based on the selection ans store it.
def _GetAssets(isLow, *args):
    global _ToolUI, _assets
    # if low is passed store in the low key and display the selection
    if isLow:
        _assets["low"] = cmds.ls(sl=True, l=True)
        cmds.textField(_ToolUI["txt_lowAssets"], e=True, text = str(cmds.ls(sl=True)))
        pass
    else:
        _assets["normal"] = cmds.ls(sl=True, l=True)
        cmds.textField(_ToolUI["txt_normalAssets"], e=True, text = str(cmds.ls(sl=True)))
        pass

#export the assets as FBX format
def _ExportAsFBX(assets, isUnity=False, postfix="", *args):
    global _ToolUI, _path
    #select the assets
    cmds.select(assets)
    if(_path):
        #get all the necessary attributes
        folder=cmds.textField(_ToolUI["txt_folderName"], q=True, text=True)
        if not cmds.file("%s/%s" % (_path, folder), q=True, exists=True):
            cmds.sysFile("%s/%s" % (_path, folder), makeDir=True)
            pass
        assetName = cmds.textField(_ToolUI["txt_assetName"], q=True, text=True)
        if not assetName:
            log.warning("Asset Name is empty")
            return
        #load fbx pluginif it is not loaded
        if not cmds.pluginInfo("fbxmaya", q=True, l=True):
            cmds.loadPlugin("fbxmaya")
        #set FBXExport options
        mel.eval("FBXExportSmoothingGroups -v true")
        mel.eval("FBXExportHardEdges -v false")
        mel.eval("FBXExportTangents -v false")
        mel.eval("FBXExportSmoothMesh -v true")
        mel.eval("FBXExportInstances -v false")
        mel.eval("FBXExportReferencedContainersContent -v false")
        # Animation
        mel.eval("FBXExportAnimationOnly -v false")
        #mel.eval("FBXExportBakeComplexAnimation -v false")
        #mel.eval("FBXExportBakeComplexStart -v 0")
        #mel.eval("FBXExportBakeComplexEnd -v 24")
        #mel.eval("FBXExportBakeComplexStep -v 1")
        #mm.eval("FBXExportBakeResampleAll -v true")
        mel.eval("FBXExportUseSceneName -v false")
        mel.eval("FBXExportQuaternion -v euler")
        mel.eval("FBXExportShapes -v true")
        mel.eval("FBXExportSkins -v true")
        # Constraints
        mel.eval("FBXExportConstraints -v false")
        # Cameras
        mel.eval("FBXExportCameras -v false")
        # Lights
        mel.eval("FBXExportLights -v false")
        # Embed Media
        mel.eval("FBXExportEmbeddedTextures -v false")
        # Connections
        mel.eval("FBXExportInputConnections -v false")
        # Axis Conversion
        mel.eval("FBXExportUpAxis y")
        #mel.eval('FBXExport -f "%s" -s' % (path + "/" + folder + "/" + assetName + ".fbx"))
        # catch any exceptions during the process
        try:
            #make a string with all the attributes
            file = (_path + "/" + folder + "/" + assetName + postfix + ".fbx")
            #export the assets
            mel.eval('FBXExport -f "%s" -s' % file)
            #display where the file is exported
            log.info ("file " + cmds.file(file, q=True, loc=True)  + " exported" )
        except Exception as e:
            #display any exception that occurs during the process
            cmds.confirmDialog( title='FBX export Exception', message='The export failed due to %s' % e.message, button=['OK'], defaultButton='OK', icn="critical" )
    else:
        log.warning("Unity Project Directory not selected")
    pass

#checking for the selected assets existence before the export starts. a safe check
def _objExists(sel):
    if not sel:
        return False
    for obj in sel:
        if not cmds.objExists(obj):
            return False
    return True

#export starts from here. Checks all the necessary requirements before the actaul export starts
def ApplyTool(*args):
    """
        PlaceHolder method that can be used by nyTool
    """
    global _assets
    #check for the normal assets and export them individually
    if not _assets["normal"]:
        log.warning("Normal Assets not selected")
    else:
        if _objExists(_assets["normal"]):
            _ExportAsFBX(_assets["normal"], isUnity = False)
        else:
            log.error("One or more objects in Normal Assets doesn't exist")
    #check for the low-version assets and export them individually
    if not _assets["low"]:
        log.warning("Low Version Assets not selected")
    else:
        if _objExists(_assets["low"]):
            _ExportAsFBX(_assets["low"], isUnity = False, postfix="_low")
        else:
            log.error("One or more objects in Low Version Assets doesn't exist")

#close the tool window
def CloseTool(*args):
    """
        Close the UI window and any other SETTINGS that is related to the tool
    """
    cmds.deleteUI(WINDOW_UI, window=True)
    pass


def _action_tool(*args):
    ApplyTool()
    CloseTool()

#Launch UI now
UI()