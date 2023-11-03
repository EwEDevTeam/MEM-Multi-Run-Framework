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

''' <summary>
''' State machine for job mnagement. Dispatching (server) to accepting / declining (client) jobs
''' </summary>
Public Class cWorkSynchronization

    Public Sub New(protocol As cDataExchangeProtocol)
        Me.IsServer = (protocol.IsServer)
    End Sub

    Public ReadOnly Property IsServer As Boolean
    Public ReadOnly Property IsClient As Boolean
        Get
            Return Not Me.IsServer
        End Get
    End Property

    Public Sub AssignToClient(job As cJob)



    End Sub


End Class
