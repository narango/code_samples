__author__ = "Narendra 'aka' NaRango"
'''
brief : Creates a thumbnail viewer for a psd file. For use in Autodesk Maya.
note : This script is for viewing purpose only. May not work by default. Requires a Maya scene that has proper node network
node network:
1. create an unknown node with name "nyTR"
2. add a "message" data-type attribute with name "psd"
3. create a psd node and connect <psd_node>.mesage to nyTR.psd
USAGE:
1) Execute this script in Maya's Script Editor
2) It should open "Unity Exporter" window
'''
import maya.cmds as cmds
import logging
import functools

log = logging.getLogger("TF_LOG")

btnSetup = None
#group names in the psd file
layerNames = ["splatter1", "splatter2", "splatter3", "splatter4", "splatter5"]
nyTR = None
imageExt = "tif"
psdNode = None

#Create a startup UI if needed
def UI(): 
    SetupNetwork()

# Setup the networks 
def SetupNetwork(*args):
    #check for @nyTR node
    if cmds.objExists("nyTR"):
        cmds.select("nyTR")
        sel = cmds.ls(sl=True, type="unknown")
        if sel:
            nyTR = sel[0]
            #lock @nyTR node and its name
            cmds.lockNode(nyTR, l=False)
            #create .psd attr (connect psdFile to .psd in Maya, shortcut to create message attribute)
            #if not cmds.attributeQuery("psd", exists=True, node=nyTR):
            #    cmds.addAttr(nyTR, at="message", ln="psd")
            
            #get @psdNode,@mesh from @nyTR
            global psdNode
            psdNode = cmds.listConnections("%s.psd" % nyTR, d=True)
            if psdNode:
                psdNode = psdNode[0]
                print(cmds.getAttr(psdNode + ".fileTextureName"))
            psdFile = cmds.getAttr(psdNode + ".fileTextureName")
            cmds.setAttr(psdNode + ".fileTextureName", psdFile, type="string")
            #create thumbnails using psdExport
            path = cmds.workspace(q=True, rd=True) + "images/"
            try:
                for layer in layerNames:
                    cmds.psdExport(ifn = psdFile, ofn=(path+layer+"Icon."+imageExt), lsn=layer, format=imageExt, bpc=0)
            except Exception as e:
                cmds.confirmDialog( title='Thumbnail Generation Exception', message='An Exception has been caught. \n %s ' % e.message, button=['OK'], defaultButton='OK', icn="critical" )

            #generate UI based on the textures created 
            GenerateTextureUI()

    else:
        cmds.confirmDialog( title='Node Network Error', message='Node network hasn\'t been setup fot this scene. \n Please open scene with the network setup ', button=['OK'], defaultButton='OK', icn="warning" )
        print("nyTR DOESN'T exists")

    pass

 #Generates textures automatically needed to display for the UI
def GenerateTextureUI(*args):
    log.info("@ui")
    #Create a window
    win = "textureRefresher"
    if cmds.window(win, exists=True):
        cmds.deleteUI(win, window=True)

    win = cmds.window(win, title="Texture Refresher", wh=(545, 350), sizeable=False, mxb=False)
    _mainForm = cmds.formLayout("mainForm", nd=100)
    width = (545 /3) - 15
    cmds.rowColumnLayout( numberOfColumns=3, columnWidth=[(1, width), (2, width), (3, width)])
    #get path from current Directory
    path = cmds.workspace(q=True, rd=True) + "images/"
    #create image buttons using icons exported from @psdFile
    for i in range(0, len(layerNames)):
        cmds.symbolButton(image=(path + layerNames[i] + "Icon." + imageExt), w=width, h=width, c=functools.partial(SetLayerForPSD, i), ann="Swap Texture with Blood_Splatter %s" % str(i+1))

    #create a button to refresh thumbnails
    cmds.button(l="Refresh", c=SetupNetwork, bgc=(0.2,0.0,0), ebg=True, ann="Refresh Thumbnails")


    cmds.showWindow(win)
    cmds.window(win, edit=True, wh=(545, 350))
    pass

#Select Layers from psd based on the user selection
def SetLayerForPSD(id, *args):
    #extra check if the @psdNode exists or not. this is check is - when you open the scene and tool, and close the scene
    if cmds.objExists(psdNode):
        cmds.setAttr("%s.layerSetName" % psdNode, layerNames[id], type="string")
        log.info("layer selected is " + layerNames[id])
    else:
        cmds.confirmDialog( title='psd Connection Error', message='psd node is not connected to nyTR node. Connect the node \n [Or psd node is not Available]', button=['OK'], defaultButton='OK', icn="critical" )	

#launch UI
UI() 