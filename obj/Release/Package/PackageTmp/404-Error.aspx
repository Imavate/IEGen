<%@ Page Title="404 File Not Found" Language="C#" MasterPageFile="~/ErrorMaster.Master" AutoEventWireup="true" CodeBehind="404-Error.aspx.cs" Inherits="IEGen._404_Error" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ErrorContent" runat="server">
    <div class="container">
        <div class="jumbotron">
            <h2 class="text-danger"><i class="fa fa-exclamation-triangle" aria-hidden="true"></i> <span style="color: #222">404 Not Found</span></h2>

            <p class="lead">Unfortunately We couldn't find the page you're looking for.</p>

            <div class="clearfix" style="padding-top:20px;"></div>

            <p><a class="btn btn-success btn-lg" runat="server" href="~/Home/Index">Take Me to the Homepage</a></p>
        </div>
    </div>

</asp:Content>
