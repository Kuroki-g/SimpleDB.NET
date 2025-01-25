import java.util.*;
import java.nio.ByteBuffer;
import java.io.*;

public class ByteBufferTest {
    public static void main(String[] args) throws Exception {
        System.out.println("ByteBuffer test");
        // Allocate a direct ByteBuffer with a capacity of 4 bytes
        ByteBuffer buffer = ByteBuffer.allocateDirect(4);

        // Set an integer value at the beginning of the buffer
        buffer.putInt(0, 12345);

        // Retrieve the integer value from the buffer
        int value = buffer.getInt(0);

        // Print the retrieved value
        System.out.println("Retrieved value: " + value);
    }
}