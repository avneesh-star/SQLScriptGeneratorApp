IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usp_GetTestJson]') AND type in (N'P', N'PC'))
BEGIN
	EXEC sp_rename '[dbo].[Usp_GetTestJson]', '[dbo].[Usp_GetTestJson]_bkp_638533200431094628'
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE OR ALTER PROCEDURE [dbo].[Usp_GetTestJson]
AS
BEGIN

	DECLARE @TestData VARCHAR(MAX) = '[
	  {
	    "id": "1",
	    "label": "Name",
	    "Type": "TextBox",
	    "validation": {
	      "required": "true",
	      "min": 10,
	      "max": 50
	    }
	  },
	  {
	    "id": "2",
	    "label": "City",
	    "Type": "TextBox",
	    "validation": {
	      "required": "true",
	      "min": 5,
	      "max": 50
	    }
	  },
	  {
	    "id": "3",
	    "label": "Mobile",
	    "Type": "TextBox",
	    "validation": {
	      "required": "true",
	      "min": 10,
	      "max": 20
	    }
	  }
	]'

	SELECT id, [label], JSON_QUERY( [validation] ) AS [validation]
	from openJson(@testdata)
	WITH
	(
	id varchar(max),
	label varchar(max),
	validation nvarchar(max) as json
	);
END



GO
