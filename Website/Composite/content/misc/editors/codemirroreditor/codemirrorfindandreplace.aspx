﻿<?xml version="1.0" encoding="UTF-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
	<control:httpheaders ID="Httpheaders1" runat="server"/>
	<head>
		<title>${string:Composite.Web.SourceEditor:FindAndReplace.LabelTitle}</title>
		<control:styleloader ID="Styleloader1" runat="server"/>
		<control:scriptloader ID="Scriptloader1" type="sub" runat="server"/>
        <script src="codemirrorfindandreplace.js" type="text/javascript"></script>
	</head>
	<body>
        <div>
            <ui:broadcasterset>
                <ui:broadcaster id="broadcasterFindNext" isdisabled="false"/>
                <ui:broadcaster id="broadcasterReplace" isdisabled="false"/>
                <ui:broadcaster id="broadcasterReplaceAll" isdisabled="false"/>
		    </ui:broadcasterset>
		    <ui:dialogpage label="${string:Composite.Web.SourceEditor:FindAndReplace.LabelTitle}" image="${icon:composite}" height="auto" resizable="false" style="padding: 10px;" binding="CodemirrorFindAndReplace">
                <ui:flexbox>
                    <ui:fields>
                        <ui:fieldgroup>
                            <ui:field>
                                <ui:fielddesc label="${string:Composite.Web.SourceEditor:FindAndReplace.LabelFind}"/>                            
                                <ui:fielddata>  
                                    <ui:datainput id="searchFor" default="true">
                                    </ui:datainput>
                                </ui:fielddata>
                            </ui:field>
                            <ui:field>
                                <ui:fielddesc label="${string:Composite.Web.SourceEditor:FindAndReplace.LabelReplaceWith}"/>
                                <ui:fielddata>
                                    <ui:datainput id="replaceWith"></ui:datainput>
                                </ui:fielddata>
                            </ui:field>
                            <ui:field>
                                <ui:fielddesc label="${string:Composite.Web.SourceEditor:FindAndReplace.LabelWholeWords}"/>
                                <ui:fielddata>
                                    <ui:checkbox id="matchWholeWord"></ui:checkbox>
                                </ui:fielddata>                                
                            </ui:field>
                            <ui:field>
                                <ui:fielddesc label="${string:Composite.Web.SourceEditor:FindAndReplace.LabelMatchCase}"/>                            
                                <ui:fielddata>
                                    <ui:checkbox id="matchCase"></ui:checkbox>
                                </ui:fielddata>
                            </ui:field>
                        </ui:fieldgroup>
                    </ui:fields>
                </ui:flexbox>          
			    <ui:dialogtoolbar>
				    <ui:toolbarbody align="right" equalsize="true">
					    <ui:toolbargroup>
                            <ui:clickbutton id="buttonFindNext" label="${string:Composite.Web.SourceEditor:FindAndReplace.ButtonFind}" focusable="true" observes="broadcasterFindNext" />
                            <ui:clickbutton id="buttonReplace" label="${string:Composite.Web.SourceEditor:FindAndReplace.ButtonReplace}" focusable="true" observes="broadcasterReplace" />
                            <ui:clickbutton id="buttonReplaceAll" label="${string:Composite.Web.SourceEditor:FindAndReplace.ButtonReplaceAll}" focusable="true" observes="broadcasterReplaceAll"/>
					    </ui:toolbargroup>
				    </ui:toolbarbody>
			    </ui:dialogtoolbar>
		    </ui:dialogpage>
        </div>
	</body>
</html>