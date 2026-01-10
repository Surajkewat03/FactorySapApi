using Microsoft.AspNetCore.Mvc;
using Npgsql;
using FactorySapApi.Models;

namespace FactorySapApi.Controllers
{
    [ApiController]
    [Route("api/material")]
    public class MaterialController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MaterialController(IConfiguration config)
        {
            _config = config;
        }

        // ===============================
        // 1️⃣ Neon DB Connection Test
        // ===============================
        [HttpGet("neon-test")]
        public async Task<IActionResult> NeonTest()
        {
            try
            {
                string connStr = _config.GetConnectionString("NeonDb");
                using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                return Ok("✅ Neon Database Connected Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===============================
        // 2️⃣ SAVE DATA (INSERT)
        // ===============================
        [HttpPost("save")]
        public async Task<IActionResult> SaveMaterial([FromBody] MaterialRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InputString))
                return BadRequest("InputString is required");

            var parts = request.InputString.Split(' ');
            if (parts.Length < 2)
                return BadRequest("Invalid format. Use: BATCH MATERIAL");

            string batchNo = parts[0];
            string materialCode = parts[1];

            string connStr = _config.GetConnectionString("NeonDb");

            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO material_transactions
                (batch_no, material_code, material_name, description, weight,
                 user_qty, remarks, sap_status)
                VALUES
                (@batch, @material, 'TEST MATERIAL', 'SAVED BEFORE SAP', 10.5,
                 @qty, @remarks, 'PENDING')
                RETURNING id;
            ", conn);

            cmd.Parameters.AddWithValue("@batch", batchNo);
            cmd.Parameters.AddWithValue("@material", materialCode);
            cmd.Parameters.AddWithValue("@qty", request.Quantity);
            cmd.Parameters.AddWithValue("@remarks", request.Remarks ?? "");

            var id = await cmd.ExecuteScalarAsync();

            return Ok(new
            {
                Message = "Saved",
                Id = id,
                Status = "PENDING"
            });
        }

        // ===============================
        // 3️⃣ SEARCH (FULL DATA)
        // ===============================
        [HttpGet("search")]
        public async Task<IActionResult> SearchMaterial([FromQuery] string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return BadRequest("Search input required");

            var parts = input.Split(' ');
            if (parts.Length < 2)
                return BadRequest("Invalid format. Use: BATCH MATERIAL");

            string connStr = _config.GetConnectionString("NeonDb");

            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
        SELECT id, batch_no, material_code, material_name,
               description, weight, user_qty, remarks, sap_status
        FROM material_transactions
        WHERE batch_no = @batch
          AND material_code = @material
        ORDER BY id DESC
        LIMIT 1;
    ", conn);

            cmd.Parameters.AddWithValue("@batch", parts[0]);
            cmd.Parameters.AddWithValue("@material", parts[1]);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return NotFound("No record found");

            return Ok(new
            {
                Id = reader["id"],
                Batch = reader["batch_no"],
                Material = reader["material_code"],
                MaterialName = reader["material_name"],
                Description = reader["description"] == DBNull.Value
                    ? "N/A"
                    : reader["description"].ToString(),
                Weight = reader["weight"] == DBNull.Value
                    ? "0"
                    : reader["weight"].ToString(),
                Quantity = reader["user_qty"],
                Status = reader["sap_status"]
            });
        }


        // ===============================
        // 4️⃣ UPDATE QTY + Condition
        // ===============================
        [HttpPost("update-qty")]
        public async Task<IActionResult> UpdateQty([FromBody] UpdateQtyRequest request)
        {
            if (request.NewQty <= 0)
                return BadRequest("Quantity must be greater than zero");

            string connStr = _config.GetConnectionString("NeonDb");

            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE material_transactions
                SET user_qty = user_qty + @newQty,
                    remarks = @remarks,
                    ""Condition"" = @condition
                WHERE id = @id
                RETURNING user_qty, ""Condition"";
            ", conn);

            cmd.Parameters.AddWithValue("@newQty", request.NewQty);
            cmd.Parameters.AddWithValue("@remarks", request.Remarks ?? "");
            cmd.Parameters.AddWithValue("@condition", request.Condition ?? "");
            cmd.Parameters.AddWithValue("@id", request.Id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return NotFound("Record not found");

            return Ok(new
            {
                Message = "Updated Successfully",
                FinalQty = reader["user_qty"],
                Condition = reader["Condition"]
            });
        }
            [HttpPost("insert")]
            public async Task<IActionResult> InsertMaterial([FromBody] InsertMaterialRequest req)
            {
                string connStr = _config.GetConnectionString("NeonDb");

                using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                var cmd = new NpgsqlCommand(@"
        INSERT INTO material_transactions
        (batch_no, material_code, material_name, description, weight, user_qty, sap_status)
        VALUES
        (@batch, @material, @name, @desc, @weight, @qty, 'PENDING')
        RETURNING id;
    ", conn);

                cmd.Parameters.AddWithValue("@batch", req.BatchNo);
                cmd.Parameters.AddWithValue("@material", req.MaterialCode);
                cmd.Parameters.AddWithValue("@name", req.MaterialName);
                cmd.Parameters.AddWithValue("@desc", req.Description);
                cmd.Parameters.AddWithValue("@weight", req.Weight);
                cmd.Parameters.AddWithValue("@qty", req.Quantity);

                var id = await cmd.ExecuteScalarAsync();

                return Ok(new
                {
                    Message = "Inserted Successfully",
                    Id = id
                });
            }

        }
    }
