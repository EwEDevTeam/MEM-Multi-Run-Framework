' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM multi-run framework prototype integrates with Ecopath with Ecosim (EwE).
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports System.Runtime.Serialization

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Server to client run payload. The payload contains a single <see cref="cJob"/>
''' that the client needs to run.
''' </summary>
''' ===========================================================================
<Serializable()>
Public Class cRun
    Inherits cSerializableObject

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="clientID">Target client ID</param>
    ''' <param name="job">The job to execute</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(clientID As Int32, job As cJob)
        Me.ClientID = clientID
        Me.Job = job
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The client these instructions are intended for.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ClientID As Int32 = 0

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The job that the client must execture.</summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Job As cJob = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' For debugging purposes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ToString() As String
        Dim s As String = "Run"
        If (Me.Job Is Nothing) Then
            s &= " (terminate)"
        Else
            s &= " " & Me.Job.Name
        End If
        Return s
    End Function

#End Region ' Public access

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Dim t As Type = GetType(cTask)
            Me.ClientID = info.GetInt32("id")
            Me.Job = DirectCast(info.GetValue("job", GetType(cJob)), cJob)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cRun", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("id", Me.ClientID)
        info.AddValue("job", Me.Job)
    End Sub

#End Region ' Serialization

End Class
