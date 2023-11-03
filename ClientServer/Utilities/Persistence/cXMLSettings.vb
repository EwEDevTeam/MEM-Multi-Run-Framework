' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM run framework is distributed in the hope that it will be useful, but 
' WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
' FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
' details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http:'www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 2020- 
'    Ecopath International Initiative, Barcelona, Spain
' ==============================================================================='

#Region " Imports "

Option Strict On
Imports System.IO
Imports System.Xml

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' INI file reader alternative using XML formats.
''' </summary>
''' <remarks>
''' Logic copied from EwE utilities.
''' </remarks>
''' -----------------------------------------------------------------------
Public Class cXMLSettings

#Region " Private vars "

    Private m_strFileName As String = ""
    Private m_doc As XmlDocument = Nothing

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New()
        ' NOP
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Die! Die!
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected Overrides Sub Finalize()
        Me.Flush()
        MyBase.Finalize()
    End Sub

#End Region ' 

#Region " Public access "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Load settings from file.
    ''' </summary>
    ''' <param name="strFileName">Name of the file to open. A ".xml" extension
    ''' is assumed if none is provide.</param>
    ''' <param name="bOverwrite">If true, this will not load the target file, 
    ''' but will overwrite it instead 
    ''' if it exists. If set to false, any existing file will be loaded.</param>
    ''' -------------------------------------------------------------------
    Public Sub Create(strFileName As String, Optional bOverwrite As Boolean = False)

        ' Add extension to file name if missing
        If String.IsNullOrWhiteSpace(Path.GetExtension(strFileName)) Then
            strFileName = Path.ChangeExtension(strFileName, ".xml")
        End If

        If (File.Exists(strFileName)) Then
            Try
                Me.EnsureHasDoc()
                Me.m_doc.Load(strFileName)
            Catch ex As Exception
            End Try
        End If
        Me.m_strFileName = strFileName

    End Sub

    Public Sub Load(strXML As String)

        If (String.IsNullOrWhiteSpace(strXML)) Then Return

        Try
            Dim xn As XmlNode = Nothing
            Me.EnsureHasDoc()
            Me.m_doc = Me.NewDoc("sections", xn)
            xn.InnerXml = strXML
        Catch ex As Exception

        End Try
    End Sub

    Public Event OnSettingsChanged(sender As Object, args As EventArgs)

    Public Function ClearSettings(strSection As String) As Boolean

        Debug.Assert(Not String.IsNullOrWhiteSpace(strSection))

        Dim node As XmlNode = Nothing

        Try
            node = m_doc.SelectSingleNode("/sections/section[@name='" & strSection & "']")
            If (node IsNot Nothing) Then
                node.RemoveAll()
                Return True
            End If
        Catch ex As Exception
            ' NOP
        End Try
        Return False

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Save a setting to the configuration file.
    ''' </summary>
    ''' <param name="strSection">Name of the section to write to. Cannot be
    ''' left empty.</param>
    ''' <param name="strKey">Name of the key to write to. Cannot be left
    ''' empty.</param>
    ''' <param name="value">The  value to write.</param>
    ''' -------------------------------------------------------------------
    Public Sub WriteSetting(Of T)(strSection As String,
                                  strKey As String,
                                  value As T)

        ' Sanity checks
        Debug.Assert(Not String.IsNullOrWhiteSpace(strSection))
        Debug.Assert(Not String.IsNullOrWhiteSpace(strKey))

        Dim node As XmlNode = Nothing
        Dim keynode As XmlNode = Nothing
        Dim val As String = Convert.ToString(value)

        Me.EnsureHasDoc()

        Try
            ' Check for/create section
            node = m_doc.SelectSingleNode("/sections/section[@name='" & strSection & "']")
            If (node Is Nothing) Then
                Dim newnode As XmlNode = m_doc.CreateElement("section")
                Dim att As XmlAttribute = m_doc.CreateAttribute("name")
                att.Value = strSection
                newnode.Attributes.Append(att)
                node = Me.m_doc.SelectSingleNode("/sections")
                node.AppendChild(newnode)
                node = Me.m_doc.SelectSingleNode("/sections/section[@name='" & strSection & "']")

                Debug.Assert(newnode.Attributes.Count = 1)
            End If

            Debug.Assert(node IsNot Nothing)

            ' get key
            keynode = m_doc.SelectSingleNode("/sections/section[@name='" & strSection & "']/item[@key='" & strKey & "']")
            If (keynode Is Nothing) Then
                ' create key
                Dim newnode As XmlNode = m_doc.CreateElement("item")
                Dim att As XmlAttribute = m_doc.CreateAttribute("key")
                att.Value = strKey
                newnode.Attributes.Append(att)
                att = m_doc.CreateAttribute("value")
                att.Value = val
                newnode.Attributes.Append(att)
                node.AppendChild(newnode)

                RaiseEvent OnSettingsChanged(Me, New EventArgs())
            Else
                ' Just update key value
                If (String.Compare(keynode.Attributes("value").Value, val, False) <> 0) Then
                    keynode.Attributes("value").Value = val
                    RaiseEvent OnSettingsChanged(Me, New EventArgs())
                End If
            End If
        Catch ex As Exception

        End Try

        ' Delete empty section nodes. This should not be necessary, but the code above seems to generate section nodes without name.
        ' No obvious why; band-aid solution will have to do for now.
        If (True) Then
            Dim sections As XmlNode = Me.m_doc.SelectSingleNode("/sections")
            Dim l As New List(Of XmlNode)
            For Each s As XmlNode In sections.ChildNodes()
                If s.ChildNodes.Count = 0 Then l.Add(s)
            Next
            For Each s As XmlNode In l
                sections.RemoveChild(s)
            Next
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Retrieve a setting from the configuration file.
    ''' </summary>
    ''' <param name="strSection">Name of the section to read from. Cannot be
    ''' left empty.</param>
    ''' <param name="strKey">Name of the key to read from. Cannot be left
    ''' empty.</param>
    ''' <param name="defaultValue">Default value to return if the indicated
    ''' key could not be found in the indicated section.</param>
    ''' -------------------------------------------------------------------
    Public Function ReadSetting(Of T)(strSection As String,
                                    strKey As String,
                                    defaultValue As T) As T

        ' Sanity checks
        Debug.Assert(Not String.IsNullOrWhiteSpace(strSection))
        Debug.Assert(Not String.IsNullOrWhiteSpace(strKey))

        If (Me.m_doc Is Nothing) Then Return defaultValue

        Dim node As XmlNode = Nothing

        Try
            node = m_doc.SelectSingleNode("/sections/section[@name='" & strSection & "']/item[@key='" & strKey & "']")
            If (node IsNot Nothing) Then
                If (GetType(T).IsEnum) Then
                    Return cEnumUtils.EnumParse(node.Attributes("value").Value, defaultValue)
                End If
                Return CType(Convert.ChangeType(node.Attributes("value").Value, GetType(T)), T)
            End If
        Catch ex As Exception
        End Try
        Return defaultValue

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Save settings to disk.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub Flush()
        Try
            Dim fld As String = Path.GetDirectoryName(Me.m_strFileName)
            If Not Directory.Exists(fld) Then Directory.CreateDirectory(fld)

            If Not String.IsNullOrWhiteSpace(Me.m_strFileName) Then
                Me.m_doc.Save(Me.m_strFileName)
            End If
        Catch ex As Exception
            ' Whoah
        End Try
    End Sub

    Public Overrides Function ToString() As String
        If (Me.m_doc Is Nothing) Then Return ""
        Return m_doc.SelectSingleNode("/sections").InnerXml
    End Function

#End Region ' Public access

#Region " Internals "

    Private Sub EnsureHasDoc()
        If (Me.m_doc Is Nothing) Then
            Me.m_doc = Me.NewDoc("sections")
        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strRootElement"></param>
    ''' <param name="xnRoot"></param>
    ''' <param name="strEncoding"></param>
    ''' <returns></returns>
    Private Function NewDoc(strRootElement As String,
                                  Optional ByRef xnRoot As XmlNode = Nothing,
                                  Optional strEncoding As String = "") As XmlDocument
        Dim doc As New XmlDocument()
        Dim xnData As XmlElement = Nothing
        Dim xaData As XmlAttribute = Nothing
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", strEncoding, "yes"))
        xnRoot = doc.CreateElement(strRootElement)
        doc.AppendChild(xnRoot)
        Return doc
    End Function

#End Region ' Internals

End Class