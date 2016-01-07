__author__ = "Narendra"
'''
Text Parser
A GUI program designed to replace characters using characters given.
user has flexible to change both find and replace characters which is upto 5

limitations:
1. use can only type one character(not a word or sentence). possible to upgrade program
2. undo not avialble. The data is only saved when user saves it. Not a major problem
'''
import sys
import os
from PySide import QtGui, QtCore, QtUiTools
from PySide import QtXml #declared because of pyinstaller importing issues

'''
A PySide Window that shows all the grpahic elements needed for TextParsing
'''
class TextParser(QtGui.QMainWindow):
    def __init__(self):
        super(TextParser, self).__init__()
        self.initUI()
    #Initialze all UI's    
    def initUI(self):
        #get the UI filename that exits in the same directory as executable      
        uifilename = os.getcwd() + "\\app.ui"
        #load ui file and process it
        loader = QtUiTools.QUiLoader()
        uifile = QtCore.QFile(uifilename)
        uifile.open(QtCore.QFile.ReadOnly)
        self.ui = loader.load(uifile, self)
        uifile.close()

        #declare any local variables needed
        self.data = ""
        self.textEdit = self.ui.textBrowser
        self.fname = None
        
        #declare and connect Open Menu
        openFile = self.ui.actionOpen       
        openFile.setShortcut('Ctrl+O')
        openFile.setStatusTip('Open File')
        openFile.triggered.connect(self.showOpenDialog)
        #declare and connect Save menu
        saveFile = self.ui.actionSave
        saveFile.setShortcut('Ctrl+S')
        saveFile.setStatusTip('Save File')
        saveFile.triggered.connect(self.saveFile)
        #declare and connect #ReplaceAll button
        pushButton = self.ui.pushButton
        pushButton.setStatusTip('Replaces all characters based on inputs')
        pushButton.clicked.connect(self.parseText)
        
        #show MainWindow
        self.ui.show()

    #calls File Dialog to select any text based file
    def showOpenDialog(self):
        #TODO: Need to add filters
        self.fname, _ = QtGui.QFileDialog.getOpenFileName(self, 'Open file', os.getcwd())
        #process file and check for exceptions        
        f = None
        try:
            f = open(self.fname, 'r')
        except Exception as e:
            if not f:
                print e.message
                return
        #get file data
        with f:
            self.data = f.read()
            #add file data to text browser
            self.textEdit.setText(self.data)
    
    #saves file data upon user-request
    def saveFile(self):
        #access file and write data to file. check for excepions also
        try:
            with open(self.fname, 'w') as f:
                f.write(self.data)
        except Exception as e:
            print e.message
        pass

    #performs parse calucations
    def parseText(self):
        #get data from text inputs
        replaceDict = self.getReplaceDict()
        #check for empty data
        if(self.data is ""): 
            #msg =  QtGui.QMessageBox.warning(self, "Warning"," Please Load a text file.", QtGui.QMessageBox.Ok)
            return
        #process data
        for char in replaceDict:
            self.data = self.data.replace(char, replaceDict[char])
        #show data in text browser
        self.textEdit.setText(self.data)

    #gets data from text inputs
    def getReplaceDict(self):
        myDict = {}
        myDict[self.ui.f_lineEdit1.text()] = self.ui.r_lineEdit1.text()
        myDict[self.ui.f_lineEdit2.text()] = self.ui.r_lineEdit2.text()
        myDict[self.ui.f_lineEdit3.text()] = self.ui.r_lineEdit3.text()
        myDict[self.ui.f_lineEdit4.text()] = self.ui.r_lineEdit4.text()
        myDict[self.ui.f_lineEdit5.text()] = self.ui.r_lineEdit5.text()
        myDict[self.ui.f_lineEdit6.text()] = self.ui.r_lineEdit6.text()
        return myDict
                                
#call Main Window
def main():
    app = QtGui.QApplication(sys.argv)
    ex = TextParser()
    sys.exit(app.exec_())


if __name__ == '__main__':
    main()