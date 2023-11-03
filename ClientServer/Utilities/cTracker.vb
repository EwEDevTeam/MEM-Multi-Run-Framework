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

Imports System.Text

Public Class cTracker

    Private Class cTrackerJob

        Private m_step As Integer = 0
        Private m_sw As New Stopwatch()

        Public Sub New(name As String)
            Me.Name = name
            Me.m_sw.Start()
        End Sub

        Public ReadOnly Property Name As String

        Public ReadOnly Property [Step] As Integer
            Get
                Me.m_step += 1
                Return Me.m_step
            End Get
        End Property

        Public ReadOnly Property ElapsedMilliseconds() As Long
            Get
                Return Me.m_sw.ElapsedMilliseconds
            End Get
        End Property

        Public ReadOnly Property Elapsed() As String
            Get
                Return Me.m_sw.Elapsed.ToString("c")
            End Get
        End Property

    End Class

    Private m_jobs As New Stack(Of cTrackerJob)
    Private m_indent As String = ""

    Public Sub New()
        ' NOP
    End Sub

    Public Sub Start(jobname As String)
        Dim j As New cTrackerJob(jobname)
        Me.m_jobs.Push(j)
        Me.UpdateIndent()
        Me.Write(String.Format("Started {0}", jobname))
    End Sub

    Public Sub Log(jobstep As String, Optional bIsStep As Boolean = True, Optional bTimed As Boolean = True)

        If (Me.m_jobs.Count > 0) Then
            Dim j As cTrackerJob = Me.m_jobs.Peek()
            If (bIsStep) Then
                jobstep = String.Format("  - {0}: {1}", j.Step, jobstep)
            End If
            If (bTimed) Then
                jobstep = String.Format("{0} at {1}s", jobstep, j.Elapsed)
            End If
        End If
        Me.Write(jobstep)

    End Sub

    Public Function [Stop]() As String
        Dim jobname As String = ""
        If (m_jobs.Count > 0) Then
            Dim j As cTrackerJob = Me.m_jobs.Peek()
            jobname = j.Name
            Me.Log("Done " & jobname, False)
            Me.m_jobs.Pop()
            Me.UpdateIndent()
        End If
        Return jobname
    End Function

    Private Sub UpdateIndent()
        Me.m_indent = New String(" "c, Math.Max(0, Me.m_jobs.Count - 1) * 2)
    End Sub

    Private Sub Write(text As String)
        If (String.IsNullOrWhiteSpace(text)) Then
            Console.WriteLine()
        Else
            Console.WriteLine("{0}{1}", m_indent, text)
        End If
    End Sub

End Class
